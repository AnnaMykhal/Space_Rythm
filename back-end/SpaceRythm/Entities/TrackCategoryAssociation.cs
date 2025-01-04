namespace SpaceRythm.Entities;

public class TrackCategoryAssociation
{
    public int TrackId { get; set; }
    public Track Track { get; set; }

    public string CategoryId { get; set; }
    public TrackCategory TrackCategory { get; set; }
}
