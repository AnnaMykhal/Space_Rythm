using Microsoft.AspNetCore.Mvc;
//using SpaceRythm.Attributes;
using SpaceRythm.Interfaces;
using SpaceRythm.Entities;
using SpaceRythm.Models.Track;
using Microsoft.EntityFrameworkCore;
using SpaceRythm.Services;
using System.Linq;
using SpaceRythm.Data;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace SpaceRythm.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TracksController : ControllerBase
{
    private readonly ITrackService _trackService;
    private readonly IUserService _userService;
    private readonly IFileService _fileService;
    private readonly IArtistService _artistService;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private IUrlHelper _urlHelper;

    public TracksController(ITrackService trackService, IUserService userService, IFileService fileService, IArtistService artistService, IUrlHelperFactory urlHelperFactory)
    {
        _trackService = trackService;
        _userService = userService;
        _fileService = fileService;
        _artistService = artistService;
        _urlHelperFactory = urlHelperFactory;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _urlHelper = _urlHelperFactory.GetUrlHelper(ControllerContext);
    }


    // Завантажити трек *
    [Authorize]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadTrack([FromForm] IFormFile file, [FromForm] IFormFile image, [FromForm] CreateTrackRequest req)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userIdInt = int.Parse(userId);
            if (userId == null)
            {
                return Unauthorized("User ID not found.");
            }

            //var analysisResult = AnalyzeTrackFile(file);
            //if (!analysisResult.IsValid)
            //{
            //    return BadRequest(analysisResult.Message);
            //}

            var track = await _trackService.UploadTrackAsync(file, image, req, userIdInt);
            return CreatedAtAction(nameof(GetTrackById), new { id = track.TrackId }, track);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private (bool IsValid, string Message) AnalyzeTrackFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return (false, "File is required.");
        }

        var allowedExtensions = new[] { ".mp3", ".wav" };
        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(extension))
        {
            return (false, "Only .mp3 and .wav files are allowed.");
        }

        const long maxFileSize = 20 * 1024 * 1024; // 20 MB
        if (file.Length > maxFileSize)
        {
            return (false, "File size exceeds 20 MB limit.");
        }

        try
        {
            using (var stream = file.OpenReadStream())
            using (var reader = new NAudio.Wave.WaveFileReader(stream))
            {
                var duration = reader.TotalTime;
                if (duration.TotalSeconds < 10)
                {
                    return (false, "Track duration is too short (less than 10 seconds).");
                }
            }
        }
        catch (Exception ex)
        {
            return (false, $"Error analyzing file: {ex.Message}");
        }

        return (true, string.Empty);
    }

    // Отримати трек за назвою (повертає лише метадані) *
    [HttpGet("by-title/{title}")]
    public async Task<IActionResult> GetTrackByTitle(string title)
    {
        var track = await _trackService.GetTrackByTitle(title);

        if (track == null)
        {
            return NotFound("Track not found");
        }
        track.Artist = await _artistService.GetById(track.ArtistId);

        var response = new PlainTrackResponse(track, track.Artist?.Name ?? "Unknown Artist",
        Url.Action("StreamTrackFile", new { id = track.TrackId }),
        Url.Action("GetTrackImage", new { fileName = track.ImagePath }));

        return Ok(response);
    }
   
    // Отримати трек за ідентифікатором (повертає лише метадані) *
    [HttpGet("by-id/{id}")]
    public async Task<IActionResult> GetTrackById(int id)
    {
        var track = await _trackService.GetById(id);
        if (track == null)
        {
            return NotFound("Track not found");
        }

        var response = new PlainTrackResponse(
        track,
        track.Artist?.Name ?? "Unknown Artist",
        Url.Action("StreamTrackFile", "Tracks", new { id = track.TrackId }, Request.Scheme),
        Url.Action("GetTrackImage", "Tracks", new { fileName = track.ImagePath }, Request.Scheme)
        );

        return Ok(response);
    }

    //Список треків по виконавцю (tracks?artistId=10), категорії (tracks?categoryId=5), обидва(tracks?categoryId=5&artistId=3) *
    [HttpGet]
    public async Task<IEnumerable<PlainTrackResponse>> Get(int? categoryId, int? artistId)
    {
        return await _trackService.GetAllTracks(categoryId, artistId);
    }

    // Пошук треків за назвою, виконавцем, категорією, жанром (повертає лише метадані) *
    [HttpGet("search")]
    public async Task<IActionResult> SearchTracks(string query)
    {
        var urlHelper = Url;
        var tracks = await _trackService.SearchByTitleOrArtist(query, urlHelper);

        if (tracks == null || !tracks.Any())
        {
            return NotFound("No tracks found matching the search criteria.");
        }

        return Ok(tracks);
    }

    // Потокове відстеження файлу за ідентифікатором (прямий доступ до файлу) *
    [HttpGet("stream/{id}")]
    public async Task<IActionResult> StreamTrackFile(int id)
    {
        var track = await _trackService.GetById(id);
        if (track == null)
        {
            return NotFound("Track not found");
        }

        var filePath = await _fileService.GetFilePathAsync(id);
        if (string.IsNullOrEmpty(filePath) || !_fileService.FileExists(filePath))
        {
            return NotFound("File not found on the server");
        }

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("File not found on the server");
        }

        const string mimeType = "audio/mpeg";
        return PhysicalFile(filePath, mimeType, $"{track.Title}.mp3");
    }

    // Перевірка прав доступу
    private async Task<bool> UserHasAccessToTrack(int trackId)
    {
        var track = await _trackService.GetById(trackId); 
        if (track == null)
        {
            return false; 
        }

        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");

        return isAdmin || (track.UserId).ToString() == currentUserId;
    }

    // Змінити дані треку 
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateTrackRequest req, [FromForm] IFormFile file)
    {
        if (!await UserHasAccessToTrack(id))
        {
            return Forbid(); 
        }
        await _trackService.Update(id, req);

        return new StatusCodeResult(204);
    }

    // Видалення треку
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await UserHasAccessToTrack(id))
        {
            return Forbid(); // Заборонити доступ
        }
        await _trackService.DeleteById(id);
        return NoContent();
    }

    // Список всіх треків без url FilePath, ImagePath *
    [HttpGet("plain")]
    public async Task<IEnumerable<PlainTrackResponse>> GetPlainSongs()
    {
        var tracks = await _trackService.GetPlainSongs();
        var res = new List<PlainTrackResponse>();

        foreach (var track in tracks)
        {
            var artist = await _artistService.GetById(track.ArtistId);

            var trackResponse = new PlainTrackResponse(
            track,
            track.Artist?.Name ?? "Unknown Artist", 
            track.FilePath, 
            track.ImagePath 
        );

            res.Add(trackResponse);
        }

        return res;
    }

    // Список останніх доданих треків
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestTracks(int count = 10)
    {
        //var tracks = await _trackService.GetRecentTracks(count);
        var tracks = await _trackService.GetRecentTracks(count, Url);
        if (tracks == null || !tracks.Any())
        {
            return NotFound("No tracks found.");
        }

        return Ok(tracks);
    }

    // Список рекомендованих треків
    [Authorize]
    [HttpGet("recommended")]
    public async Task<IActionResult> GetRecommendedTracks(int count = 10)
    {
        var userId = GetCurrentUserId(); 
        if (userId == null)
        {
            return Unauthorized("User is not authenticated.");
        }

        var recommendedTracks = await _trackService.GetRecommendedTracks(userId.Value, count, Url);

        if (recommendedTracks == null || !recommendedTracks.Any())
        {
            var recentTracks = await _trackService.GetRecentTracks(count, Url);
            if (recentTracks == null || !recentTracks.Any())
            {
                return NotFound("No tracks available.");
            }
            return Ok(recentTracks); 
        }

        return Ok(recommendedTracks);
    }

    // Список топ треків
    [HttpGet("top")]
    public async Task<IActionResult> GetTopTracksByPlays(int count = 10)
    {
        var tracks = await _trackService.GetTopTracksByPlays(count, Url);
        if (tracks == null || !tracks.Any())
        {
            return NotFound("No top tracks found.");
        }

        return Ok(tracks);
    }

    private int? GetCurrentUserId()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
        return null;
    }

    // Відстеження зображення за ідентифікатором (прямий доступ до файлу)
    [HttpGet("image/{id}")]
    public async Task<IActionResult> StreamTrackImage(int id)
    {
        var track = await _trackService.GetById(id);
        if (track == null)
        {
            return NotFound("Track not found");
        }

        var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "trackImages");
        var imagePath = Path.Combine(rootPath, track.ImagePath);

        if (string.IsNullOrEmpty(track.ImagePath) || !System.IO.File.Exists(imagePath))
        {
            return NotFound("Image not found on the server");
        }

        var mimeType = GetMimeTypeFromFileExtension(imagePath);

        return PhysicalFile(imagePath, mimeType);
    }

    // Метод для визначення MIME-типу
    private string GetMimeTypeFromFileExtension(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream",
        };
    }

}

