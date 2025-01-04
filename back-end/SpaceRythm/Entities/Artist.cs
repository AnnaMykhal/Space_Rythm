using SpaceRythm.Models.User;
using SpaceRythm.Util;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SpaceRythm.Models.Artist;

namespace SpaceRythm.Entities;

[Table("artist")]
public class Artist
{
    [Key] 
    public int ArtistId { get; set; }

    [Required] 
    public string Name { get; set; }

    public string? Bio { get; set; }
  
    public ICollection<Track> Tracks { get; set; }

    public string? ImagePath { get; set; }
    public ICollection<ArtistCategoryLink> ArtistCategoryLinks { get; set; } = new List<ArtistCategoryLink>();
    public Artist(CreateArtistRequest req)
    {
        Name = req.Name;
        Bio = req.Bio?? "";
        Tracks = req.Tracks ?? new List<Track>();
        ArtistCategoryLinks = new List<ArtistCategoryLink>();
    }

    // Порожній конструктор, необхідний для ORM
    public Artist() { }
}
