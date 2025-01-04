namespace SpaceRythm.Models.Artist;

public class UpdateArtistRequest
{
    public string Name { get; set; }

    public string? Bio { get; set; }

    public IEnumerable<int>? CategoriesId { get; set; }

    public string? Image { get; set; }
    public IEnumerable<int> TrackIds { get; set; } = new List<int>();
}