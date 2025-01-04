using Microsoft.EntityFrameworkCore;
using SpaceRythm.Data;
using SpaceRythm.Entities;
using SpaceRythm.Interfaces;
using SpaceRythm.Models.Artist;
using SpaceRythm.Models.Track;
using NAudio.Wave;
using SpaceRythm.Controllers;
using System.Security.Policy;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Http.Extensions;


namespace SpaceRythm.Services;

public class TrackService : ITrackService
{
    private readonly ILogger<TrackService> _logger;
    private readonly MyDbContext _context;
    private readonly IArtistService _artistService;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LinkGenerator _linkGenerator;


    public TrackService(ILogger<TrackService> logger, MyDbContext context, IArtistService artistService, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor,
        LinkGenerator linkGenerator)
    {
        _logger = logger;
        _context = context;
        _artistService = artistService;
        _environment=environment;
        _httpContextAccessor=httpContextAccessor;
        _linkGenerator=linkGenerator;
    }

    public async Task<PlainTrackResponse> UploadTrackAsync(IFormFile file, IFormFile image, CreateTrackRequest req, int userId)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required.");
        }

        var allowedExtensions = new[] { ".mp3", ".wav" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
        {
            throw new ArgumentException("Only .mp3 and .wav files are allowed.");
        }

        const long maxFileSize = 20 * 1024 * 1024; // 20 MB
        if (file.Length > maxFileSize)
        {
            throw new ArgumentException("File size exceeds 20 MB limit.");
        }

        var temporaryFilePath = Path.GetTempFileName();
        using (var fileStream = new FileStream(temporaryFilePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        var temporaryDuration = GetFileDuration(temporaryFilePath);
        File.Delete(temporaryFilePath);

        var existingTrack = await _context.Tracks
            .Include(t => t.Artist)
            .FirstOrDefaultAsync(t =>
                t.Title == req.Title &&
                t.Artist.Name == req.ArtistName &&
                t.Duration == temporaryDuration
            );

        if (existingTrack != null)
        {
            _logger.LogInformation($"Track '{req.Title}' by '{req.ArtistName}' already exists in the database.");
            return null;
        }
        
        var uniqueTrackFileName = Guid.NewGuid() + extension;
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "tracks");
        Directory.CreateDirectory(uploadsFolder);
        var trackFilePath = Path.Combine(uploadsFolder, uniqueTrackFileName);

        using (var stream = new FileStream(trackFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        string uniqueImageFileName = null;
        if (image != null && image.Length > 0)
        {
            var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var imageExtension = Path.GetExtension(image.FileName).ToLower();

            if (!allowedImageExtensions.Contains(imageExtension))
            {
                throw new ArgumentException("Only .jpg, .jpeg, .png, .gif images are allowed.");
            }

            var imageFolder = Path.Combine(_environment.WebRootPath, "trackImages");
            Directory.CreateDirectory(imageFolder);

            uniqueImageFileName = Guid.NewGuid() + imageExtension;
            var imagePath = Path.Combine(imageFolder, uniqueImageFileName);
           
            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
        }

        if (string.IsNullOrEmpty(req.ArtistName))
        {
            return null;
        }

        var artist = await _context.Artists
            .Where(a => a.Name == req.ArtistName)
            .Select(a => new Artist
            {
                ArtistId = a.ArtistId,
                Name = a.Name ?? "Unknown" 
            })
            .FirstOrDefaultAsync();
             if (artist == null)
             {
                 artist = new Artist { Name = req.ArtistName };
                 _context.Artists.Add(artist);
                 await _context.SaveChangesAsync();
             }

        var track = new Track
        {
            Title = req.Title,
            Genre = req.Genre,
            Tags = req.Tags,
            Description = req.Description ?? string.Empty,
            FilePath = uniqueTrackFileName,
            Duration = GetFileDuration(trackFilePath),
            UploadDate = DateTime.Now,
            ArtistId = artist.ArtistId,
            ImagePath = uniqueImageFileName ?? string.Empty,
            UserId = userId,
            TrackCategoryLink = new List<TrackCategoryLink>()
        };

        foreach (var categoryName in req.Categories)
        {
            var category = await _context.TrackCategories.FirstOrDefaultAsync(c => c.Category == categoryName);

            if (category == null)
            {
                category = new TrackCategory { Category = categoryName };
                _context.TrackCategories.Add(category);
                await _context.SaveChangesAsync();
            }

            track.TrackCategoryLink.Add(new TrackCategoryLink { TrackCategoryId = category.Id });
        }

        _context.Tracks.Add(track);
        await _context.SaveChangesAsync();

        var trackMetadata = new TrackMetadata
        {
            TrackId = track.TrackId,
            Plays = 0,
            Likes = 0,
            CommentsCount = 0
        };

        _context.TrackMetadatas.Add(trackMetadata);
        await _context.SaveChangesAsync();

        var trackUrl = _linkGenerator.GetPathByAction(
           "StreamTrackFile",
           "Tracks",
           new { id = track.TrackId },
           new PathString("/"));

        var imageUrl = _linkGenerator.GetPathByAction(
            "StreamTrackImage",
            "Tracks",
            new { id = track.TrackId },
            new PathString("/"));

        var trackResponse = new PlainTrackResponse(
            track,
            artist.Name,
            trackUrl,  
            imageUrl ?? string.Empty 
        );
        return trackResponse;
    }
    private TimeSpan GetFileDuration(string filePath)
    {
        var reader = new NAudio.Wave.AudioFileReader(filePath);
        return reader.TotalTime;
    }

    public async Task<Track> GetTrackByTitle(string title)
    {
        return await _context.Tracks
        .Include(t => t.TrackCategoryLink)
            .ThenInclude(link => link.TrackCategory) 
        .Include(t => t.Artist) 
        .Include(t => t.TrackMetadata) 
        .FirstOrDefaultAsync(t => t.Title == title);
    }
    public async Task<IEnumerable<PlainTrackResponse>> GetAllTracks(int? categoryId = null, int? artistId = null)
    {
        IQueryable<Track> query = _context.Tracks
              .Include(t => t.Artist)
              .Include(t => t.TrackCategoryLink)
              .ThenInclude(link => link.TrackCategory) 
              .Include(t => t.TrackMetadata);


        if (categoryId.HasValue)
            query = query.Where(t => t.TrackCategoryLink.Any(link => link.TrackCategoryId == categoryId.Value));

        if (artistId.HasValue)
            query = query.Where(t => t.ArtistId == artistId.Value);

        var tracks = await query.ToListAsync();
        var httpContext = _httpContextAccessor.HttpContext;

        var urlHelperFactory = httpContext.RequestServices.GetService<IUrlHelperFactory>();

        if (urlHelperFactory == null)
        {
            _logger.LogError("IUrlHelperFactory is not available.");
            return Enumerable.Empty<PlainTrackResponse>(); ;
        }

        var actionContext = new ActionContext(
            httpContext,
            httpContext.GetRouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor() 
        );

        var urlHelper = urlHelperFactory.GetUrlHelper(actionContext);

        var trackResponses = tracks.Select(track =>
        {
            var streamTrackUrl = urlHelper.Action("StreamTrackFile", "Tracks", new { id = track.TrackId }, "https");
            var trackImageUrl = urlHelper.Action("StreamTrackImage", "Tracks", new { id = track.TrackId }, "https");

            return new PlainTrackResponse(
                track,
                track.Artist.Name,
                streamTrackUrl ?? string.Empty, 
                trackImageUrl ?? string.Empty 
            );
        }).ToList();
        return trackResponses;
    }

    public async Task<IEnumerable<Track>> GetAll()
    {
        return await _context.Tracks.ToListAsync();
    }

    public async Task<IEnumerable<Track>> GetByArtist(int artistId)
    {
        return await _context.Tracks
         .Where(a => a.ArtistId == artistId)
         .ToListAsync();
    }
    public async Task<IEnumerable<Track>> GetByCategory(int categoryId)
    {
        return await _context.Tracks
            .Where(track => track.TrackCategoryLink.Any(link => link.TrackCategoryId == categoryId))
            .ToListAsync();
    }

    public async Task<Track> GetById(int id)
    {
        var track = await _context.Tracks
        .Include(t => t.TrackCategoryLink)
            .ThenInclude(link => link.TrackCategory) 
        .Include(t => t.Artist)
        .Include(t => t.TrackMetadata)
        .FirstOrDefaultAsync(t => t.TrackId == id);

        if (track == null)
        {
            throw new KeyNotFoundException("Track not found.");
        }

        return track;
    }

    public async Task<IEnumerable<PlainTrackResponse>> SearchByTitleOrArtist(string query, IUrlHelper urlHelper)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<PlainTrackResponse>();
        }

        query = query.ToLower();

        Genre? genreSearch = null;
        if (Enum.TryParse<Genre>(query, true, out var genre))
        {
            genreSearch = genre;
        }

        var tracks = await _context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.TrackMetadata)
            .Include(t => t.TrackCategoryLink)
                .ThenInclude(link => link.TrackCategory)
            .Where(t =>
                t.Title.ToLower().Contains(query) ||
                t.Artist.Name.ToLower().Contains(query) ||
                (genreSearch.HasValue && t.Genre == genreSearch.Value) ||
                t.TrackCategoryLink.Any(link => link.TrackCategory.Category.ToLower().Contains(query))
            )
            .ToListAsync();

        return tracks.Select(track =>
            new PlainTrackResponse(
                track,
                track.Artist?.Name ?? "Unknown",
                urlHelper.Action("StreamTrackFile", "Tracks", new { id = track.TrackId }, "https") ?? string.Empty,
                urlHelper.Action("StreamTrackImage", "Tracks", new { id = track.TrackId }, "https") ?? string.Empty
            )
        ).ToList();
    }

    public async Task Update(int id, UpdateTrackRequest req)
    {
        var track = await _context.Tracks
            .Include(t => t.TrackCategoryLink) 
            .ThenInclude(tcl => tcl.TrackCategory)   
            .FirstOrDefaultAsync(t => t.TrackId == id);

        if (track == null)
        {
            throw new KeyNotFoundException("Track not found.");
        }

        track.Title = req.Title ?? track.Title;
        track.Genre = req.Genre;
        track.Tags = req.Tags;
        track.Description = req.Description ?? track.Description;
        track.FilePath = req.FilePath ?? track.FilePath;
        track.Duration = (req.Duration.HasValue && req.Duration.Value > TimeSpan.Zero) ? req.Duration.Value : track.Duration;

        if (req.CategoriesId != null && req.CategoriesId.Any())
        {
            var existingCategoryLinks = track.TrackCategoryLink.ToList();
            foreach (var link in existingCategoryLinks)
            {
                if (!req.CategoriesId.Contains(link.TrackCategoryId))
                {
                    _context.TrackCategoryLink.Remove(link);
                }
            }

            foreach (var categoryId in req.CategoriesId)
            {
                if (!track.TrackCategoryLink.Any(tcl => tcl.TrackCategoryId == categoryId))
                {
                    track.TrackCategoryLink.Add(new TrackCategoryLink
                    {
                        TrackId = track.TrackId,
                        TrackCategoryId = categoryId
                    });
                }
            }
        }

        _context.Tracks.Update(track);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Track>> GetPlainSongs()
    {
        var tracks = await _context.Tracks
            .Include(t => t.TrackCategoryLink)
            .ThenInclude(link => link.TrackCategory)
        .Include(t => t.Artist)
        .Include(t => t.TrackMetadata).ToListAsync();
        return tracks;
    }
    
    public async Task Delete(int id)
    {
        var track = await _context.Tracks.FindAsync(id);

        if (track == null)
        {
            throw new KeyNotFoundException("Track not found.");
        }

        _context.Tracks.Remove(track);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteById(int id)
    {
        var track = await _context.Tracks.FindAsync(id);
        if (track == null)
        {
            throw new KeyNotFoundException("Track not found.");
        }
        _context.Tracks.Remove(track);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<PlainTrackResponse>> GetRecentTracks(int count = 10, IUrlHelper urlHelper = null)
    {
        var recentTracks = await _context.Tracks
             .Include(t => t.Artist)
             .Include(t => t.TrackMetadata)
             .Include(t => t.TrackCategoryLink)
             .ThenInclude(link => link.TrackCategory)
             .OrderByDescending(t => t.UploadDate) 
             .Take(count) 
             .ToListAsync();

        return recentTracks.Select(track => new PlainTrackResponse(
              track,
              track.Artist?.Name ?? "Unknown",
              filePath: urlHelper?.Action("StreamTrackFile", "Tracks", new { id = track.TrackId }, "https")
                         ?? $"https://localhost:5017/api/tracks/stream/{track.TrackId}",
              imagePath: urlHelper?.Action("StreamTrackImage", "Tracks", new { id = track.TrackId }, "https")
                         ?? $"https://localhost:5017/trackImages/default-image.jpg" 
        )).ToList();
    }

    public async Task<IEnumerable<PlainTrackResponse>> GetRecommendedTracks(int userId, int count = 10, IUrlHelper urlHelper = null)
    {
        // Отримуємо треки, які лайкнув поточний користувач
        var likedTrackIds = await _context.Likes
            .Where(l => l.UserId == userId)
            .Select(l => l.TrackId)
            .ToListAsync();

        // Знаходимо користувачів, які лайкнули ті ж треки
        var similarUserIds = await _context.Likes
            .Where(l => likedTrackIds.Contains(l.TrackId) && l.UserId != userId)
            .Select(l => l.UserId)
            .Distinct()
            .ToListAsync();

        // Отримуємо треки, які лайкнули схожі користувачі, але не лайкнув поточний
        var recommendedTracks = await _context.Likes
            .Where(l => similarUserIds.Contains(l.UserId) && !likedTrackIds.Contains(l.TrackId))
            .Select(l => l.Track)
            .Distinct()
            .Include(t => t.Artist)
            .Include(t => t.TrackMetadata)
            .Include(t => t.TrackCategoryLink)
            .ThenInclude(link => link.TrackCategory)
            .OrderByDescending(t => t.UploadDate) 
            .Take(count)
            .ToListAsync();

        return recommendedTracks.Select(track => new PlainTrackResponse(
            track,
            track.Artist?.Name ?? "Unknown",
            filePath: urlHelper?.Action("StreamTrackFile", "Tracks", new { id = track.TrackId }, "https")
                       ?? $"https://localhost:5017/api/tracks/stream/{track.TrackId}",
            imagePath: urlHelper?.Action("StreamTrackImage", "Tracks", new { id = track.TrackId }, "https")
                       ?? $"https://localhost:5017/trackImages/default-image.jpg"
        )).ToList();
    }

    public async Task<IEnumerable<PlainTrackResponse>> GetTopTracksByPlays(int count = 10, IUrlHelper urlHelper = null)
    {
        var topTracks = await _context.Tracks
            .Include(t => t.Artist)
            .Include(t => t.TrackMetadata)
            .Include(t => t.TrackCategoryLink)
            .ThenInclude(link => link.TrackCategory)
            .GroupJoin(
                _context.Listenings,
                track => track.TrackId,
                listening => listening.TrackId,
                (track, listenings) => new
                {
                    Track = track,
                    PlayCount = listenings.Count()
                }
            )
            .OrderByDescending(x => x.PlayCount)
            .Take(count) 
            .ToListAsync();

        return topTracks.Select(entry => new PlainTrackResponse(
            entry.Track,
            entry.Track.Artist?.Name ?? "Unknown",
            filePath: urlHelper?.Action("StreamTrackFile", "Tracks", new { id = entry.Track.TrackId }, "https")
                      ?? $"https://localhost:5017/api/tracks/stream/{entry.Track.TrackId}",
            imagePath: urlHelper?.Action("StreamTrackImage", "Tracks", new { id = entry.Track.TrackId }, "https")
                      ?? $"https://localhost:5017/trackImages/default-image.jpg"
        )).ToList();
    }

    
}





//public async Task<IEnumerable<PlainTrackResponse>> GetTopTracksByLikes(int count = 10, IUrlHelper urlHelper = null)
//{
//    Console.WriteLine($"TrackService GetTopTracksByLikes.");

//    var topTracks = await _context.Tracks
//         .Include(t => t.Artist)
//         .Include(t => t.TrackMetadata)
//         .Include(t => t.TrackCategoryLink)
//         .ThenInclude(link => link.TrackCategory)
//         .Include(t => t.Likes)  // Включаємо лайки
//         .OrderByDescending(t => t.Likes.Count) // Сортуємо за кількістю лайків
//         .Take(count) // Вибираємо топ N
//         .ToListAsync();

//    return topTracks.Select(track => new PlainTrackResponse(
//          track,
//          track.Artist?.Name ?? "Unknown",
//          filePath: urlHelper?.Action("StreamTrackFile", "Tracks", new { id = track.TrackId }, "https")
//                     ?? $"https://localhost:5017/api/tracks/stream/{track.TrackId}",
//          imagePath: urlHelper?.Action("StreamTrackImage", "Tracks", new { id = track.TrackId }, "https")
//                     ?? $"https://localhost:5017/trackImages/default-image.jpg"
//    )).ToList();
//}

