using System.ComponentModel.DataAnnotations;
using SpaceRythm.Entities;

namespace SpaceRythm.Models.Artist;

public class CreateArtistRequest
{
    [Required]
    public string Name { get; set; }
    public string? Bio { get; set; }

    public string? ImagePath { get; set; }
    public ICollection<SpaceRythm.Entities.Track>? Tracks { get; set; }
}

