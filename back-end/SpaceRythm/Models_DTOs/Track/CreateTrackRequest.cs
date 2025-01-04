using SpaceRythm.Data;
using System.ComponentModel.DataAnnotations;

namespace SpaceRythm.Models.Track;


public class CreateTrackRequest
{
    [Required]
    public string Title { get; set; } 

    [Required]
    public string ArtistName { get; set; }  

    public Genre Genre { get; set; }
    public List<string>? Tags { get; set; }
    public string? Description { get; set; }
    public List<string> Categories { get; set; }
}
