using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace SpaceRythm.Entities;

public class TrackCategoryLink
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Track")]
    public int TrackId { get; set; }
    public Track Track { get; set; }

    [ForeignKey("TrackCategory")]
    public int TrackCategoryId { get; set; }
    public TrackCategory TrackCategory { get; set; }
}