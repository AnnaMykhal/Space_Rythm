using SpaceRythm.Models.Category;
using SpaceRythm.Interfaces;
using SpaceRythm.Data;
using Microsoft.EntityFrameworkCore;
using SpaceRythm.Entities;
using SpaceRythm.Models.Track;

namespace SpaceRythm.Services;

public class TrackCategoryService : ITrackCategoryService
{
    private readonly MyDbContext _context;
    private readonly string _imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "categoryImages");

    public TrackCategoryService(MyDbContext context)
    {
        _context = context;
    }

    public async Task Create(CreateCategoryRequest req)
    {
        if (req.Image != null && req.Image.Length > 0)
        {
            var imageUrl = await SaveImage(req.Image);
            req.ImageUrl = imageUrl;
        }
        else
        {
            req.ImageUrl = null;
        }

        var existingCategory = await _context.TrackCategories
            .FirstOrDefaultAsync(cat => cat.Category == req.Category);

        if (existingCategory != null)
        {
            throw new InvalidOperationException($"Category '{req.Category}' already exists.");
        }

        var category = new TrackCategory { Category = req.Category, ImageUrl = req.ImageUrl };

        _context.TrackCategories.Add(category);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TrackCategory>> GetAll()
    {
        return await _context.TrackCategories.ToListAsync();
    }

    public async Task<TrackCategory> GetById(int id)
    {
        return await _context.TrackCategories.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task DeleteById(int id)
    {
        var category = await _context.TrackCategories.FindAsync(id);

        if (category != null)
        {
            _context.TrackCategories.Remove(category);
            await _context.SaveChangesAsync();  
        }
    }

    public async Task<string> SaveImage(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            return null;
        }

        if (!Directory.Exists(_imageFolderPath))
        {
            Directory.CreateDirectory(_imageFolderPath);
        }

        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
        var filePath = Path.Combine(_imageFolderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        return "/categoryImages/" + fileName;
    }
}
