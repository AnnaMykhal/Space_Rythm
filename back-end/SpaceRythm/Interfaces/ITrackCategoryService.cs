using SpaceRythm.Models.Category;
using SpaceRythm.Entities;

namespace SpaceRythm.Interfaces;

public interface ITrackCategoryService
{
    Task Create(CreateCategoryRequest category);
    Task<IEnumerable<TrackCategory>> GetAll();
    Task<TrackCategory> GetById(int id);
    Task DeleteById(int id);
    Task<string> SaveImage(IFormFile image);
}
