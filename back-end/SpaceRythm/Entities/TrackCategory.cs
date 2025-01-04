using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SpaceRythm.Entities;

public class TrackCategory
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Category { get; set; }
    public string? ImageUrl { get; set; }
    public ICollection<CategoryLiked> CategoriesLiked { get; set; } = new List<CategoryLiked>();
    public ICollection<TrackCategoryLink> TrackCategoryLinks { get; set; } = new List<TrackCategoryLink>();
    public ICollection<ArtistCategoryLink> ArtistCategoryLinks { get; set; } = new List<ArtistCategoryLink>();
}
