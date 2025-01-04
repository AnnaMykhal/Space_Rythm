using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using SpaceRythm.Data;
using SpaceRythm.Entities;
using SpaceRythm.Interfaces;
using SpaceRythm.Models.Artist;
using SpaceRythm.Models.User;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


namespace SpaceRythm.Services;

public class ArtistService : IArtistService
{
    private readonly MyDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _environment;

    public ArtistService(MyDbContext context, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _environment=environment;
    }

    // Створення користувача *
    public async Task Create(IFormFile image, CreateArtistRequest req)
    {
        string uniqueImageFileName = null;
        if (image != null && image.Length > 0)
        {
            var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var imageExtension = Path.GetExtension(image.FileName).ToLower();

            if (!allowedImageExtensions.Contains(imageExtension))
            {
                throw new ArgumentException("Only .jpg, .jpeg, .png, .gif images are allowed.");
            }

            var imageFolder = Path.Combine(_environment.WebRootPath, "artistImages");
            Directory.CreateDirectory(imageFolder);

            uniqueImageFileName = Guid.NewGuid() + imageExtension;
            var imagePath = Path.Combine(imageFolder, uniqueImageFileName);

            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
        }

        var artist = new Artist
        {
            Name = req.Name,
            ImagePath = uniqueImageFileName ?? string.Empty
        };
       
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();
    }
       
    public async Task Delete(int id)
    {
        var artist = await _context.Artists.FindAsync(id);

        _context.Artists.Remove(artist);
        await _context.SaveChangesAsync();
    }
   
    public async Task<IEnumerable<Artist>> GetAll()
    {
        return await _context.Artists.Include(a => a.Tracks).ToListAsync();
    }

    public async Task<Artist> GetById(int id)
    {
        var artist = await _context.Artists
        .Where(a => a.ArtistId == id)
        .FirstOrDefaultAsync();

        return artist;
    }

    public async Task<IEnumerable<Artist>> GetByCategory(int categoryId)
    {
        return await _context.Artists
            .Include(a => a.Tracks)
            .Where(artist => artist.ArtistCategoryLinks.Any(link => link.CategoryId == categoryId))
            .ToListAsync();
    }

    public async Task UpdateAsync(int id, UpdateArtistRequest req)
    {
        var artist = await _context.Artists
            .Include(a => a.Tracks) 
            .FirstOrDefaultAsync(a => a.ArtistId == id);

        if (artist == null)
        {
            throw new Exception("Artist not found");
        }

        if (!string.IsNullOrEmpty(req.Name) && !string.IsNullOrWhiteSpace(req.Name))
        {
            artist.Name = req.Name;
        }

        if (!string.IsNullOrEmpty(req.Bio) && !string.IsNullOrWhiteSpace(req.Bio))
        {
            artist.Bio = req.Bio; 
        }

        if (!string.IsNullOrEmpty(req.Image) && !string.IsNullOrWhiteSpace(req.Image))
        {
            artist.ImagePath = req.Image;
        }

        if (req.CategoriesId != null)
        {
            foreach (var categoryId in req.CategoriesId)
            {
                artist.ArtistCategoryLinks.Add(new ArtistCategoryLink
                {
                    CategoryId = categoryId,
                    ArtistId = artist.ArtistId 
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    
    public async Task<IEnumerable<PlainArtistResponse>> GetPlainArtists(IEnumerable<Entities.Artist> artists)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            throw new InvalidOperationException("HttpContext is not available.");
        }

        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";

        var artistList = await _context.Artists
        .Include(a => a.Tracks) 
        .ToListAsync();

        return artistList.Select(artist => new PlainArtistResponse(artist, baseUrl)).ToList();
    }
    
}
