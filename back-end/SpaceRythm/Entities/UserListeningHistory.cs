using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SpaceRythm.Entities;

[Table("user_listening_history")]
public class UserListeningHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }

    [Required]
    public int TrackId { get; set; }

    [ForeignKey("TrackId")]
    public Track Track { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }
}
