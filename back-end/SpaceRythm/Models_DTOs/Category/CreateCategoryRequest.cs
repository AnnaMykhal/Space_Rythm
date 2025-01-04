using System.ComponentModel.DataAnnotations;

namespace SpaceRythm.Models.Category;

public class CreateCategoryRequest
{
    [Required]
    public string Category { get; set; }
    public IFormFile Image { get; set; }
    public string? ImageUrl { get; set; }
}