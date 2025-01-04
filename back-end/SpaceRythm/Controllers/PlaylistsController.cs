using Microsoft.AspNetCore.Mvc;
using SpaceRythm.Entities;
using SpaceRythm.Interfaces;
using SpaceRythm.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SpaceRythm.Models.Playlist;
using System.ComponentModel.Design;

namespace SpaceRythm.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlaylistsController : ControllerBase
{
    private readonly IPlaylistService _playlistService;

    public PlaylistsController(IPlaylistService playlistService)
    {
        _playlistService = playlistService;
    }

    // Отримати плейлисти користувача *
    [Authorize]
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserPlaylists(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var playlists = await _playlistService.GetUserPlaylistsAsync(userId, page, pageSize);
        return Ok(playlists);
    }

    // Переглянути вміст плейлиста *
    [Authorize]
    [HttpGet("{playlistId}")]
    public async Task<IActionResult> GetPlaylist(int playlistId)
    {
        try
        {
            var playlist = await _playlistService.GetPlaylistByIdAsync(playlistId);
            return Ok(playlist);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // Створити новий плейлист *
    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreatePlaylist([FromForm] CreatePlaylistRequest request, [FromForm] IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        string imagePath = null;
        if (imageFile != null && imageFile.Length > 0)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "playlistImages");
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            Directory.CreateDirectory(folderPath);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            imagePath = $"playlistImages/{fileName}";
        }

        var playlist = await _playlistService.CreatePlaylistAsync(request.UserId, request.Name, request.Description, request.TrackIds, imagePath);

        return CreatedAtAction(nameof(GetUserPlaylists), new { userId = request.UserId }, playlist);
    }

    // Додати трек до плейлиста *
    [Authorize]
    [HttpPost("{playlistId}/add-track")]
    public async Task<IActionResult> AddTrackToPlaylist(int playlistId, [FromBody] AddTrackToPlaylistRequest request)
    {
        await _playlistService.AddTrackToPlaylistAsync(playlistId, request.TrackId);
        return NoContent();
    }

    // Видалити трек із плейлиста *
    [Authorize]
    [HttpDelete("{playlistId}/remove-track")]
    public async Task<IActionResult> RemoveTrackFromPlaylist(int playlistId, [FromBody] RemoveTrackFromPlaylistRequest request)
    {
        await _playlistService.RemoveTrackFromPlaylistAsync(playlistId, request.TrackId);
        return NoContent();
    }


    // Редагувати плейлист *
    [Authorize]
    [HttpPut("{playlistId}/edit")]
    public async Task<IActionResult> EditPlaylist(int playlistId, [FromForm] UpdatePlaylistRequest request)
    {
        string? newImageUrl = null;
        if (request.NewFile != null && request.NewFile.Length > 0)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "playlistImages");
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.NewFile.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            Directory.CreateDirectory(folderPath);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await request.NewFile.CopyToAsync(fileStream);
            }

            newImageUrl = $"playlistImages/{fileName}";
        }

        var updatedPlaylist = await _playlistService.EditPlaylistAsync(
            playlistId,
            request.NewName,
            request.NewDescription,
            request.IsPublic,
            newImageUrl);

        return Ok(updatedPlaylist);
    }


    // Видалити плейлист *
    [Authorize]
    [HttpDelete("{playlistId}")]
    public async Task<IActionResult> DeletePlaylist(int playlistId)
    {
        var playlist = await _playlistService.GetPlaylistByIdAsync(playlistId);
        if (playlist == null)
        {
            return NotFound("Playlist not found.");
        }

        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        if (playlist.UserId.ToString() != userId && !isAdmin)
        {
            return Unauthorized("You are not authorized to delete this playlist.");
        }
        await _playlistService.DeletePlaylistAsync(playlistId);
        return NoContent();
    }

    private string? GetCurrentUserId()
    {
        return User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

}
