using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SpaceRythm.Entities;

public class Listening
{
    [Key]
    public int ListeningId { get; set; } 

    [ForeignKey("Track")]
    public int TrackId { get; set; }

    public Track Track { get; set; } 

    public int? UserId { get; set; } 

    public User User { get; set; }

    public DateTime ListenedDate { get; set; } = DateTime.Now; 
}
