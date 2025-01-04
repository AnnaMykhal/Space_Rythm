namespace SpaceRythm.Entities;

public class ArtistCategoryLink
{
    public int ArtistId { get; set; }
    public int CategoryId { get; set; }

    public Artist Artist { get; set; }
    public TrackCategory Category { get; set; }
}
