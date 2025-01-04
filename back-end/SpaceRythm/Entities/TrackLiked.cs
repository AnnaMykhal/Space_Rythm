using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SpaceRythm.Entities;

[Table("tracks_liked")]
public class TrackLiked
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }

    [Required]
    public int TrackId { get; set; } // Ідентифікатор пісні
    [ForeignKey("TrackId")]
    public Track Track { get; set; }
}