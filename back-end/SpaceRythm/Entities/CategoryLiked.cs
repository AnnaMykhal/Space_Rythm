using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SpaceRythm.Entities;

[Table("categories_liked")]
public class CategoryLiked
{
    //[Key]
    //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }

    [Required]
    public int CategoryId { get; set; } // Ідентифікатор категорії
    
    [ForeignKey("CategoryId")]
    public TrackCategory TrackCategory { get; set; }
}
