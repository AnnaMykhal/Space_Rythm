using SpaceRythm.Data;

namespace SpaceRythm.Models.Track;

public class UpdateTrackRequest
{
    public string Title { get; set; }
    public int? ArtistId { get; set; }
    public string ArtistName { get; set; }
    public string? FilePath { get; set; }

    public Genre? Genre { get; set; }
    public List<string>? Tags { get; set; }
    public string? Description { get; set; }
    public TimeSpan? Duration { get; set; }
    public DateTime? UploadDate { get; set; }

    public IEnumerable<int>? CategoriesId { get; set; }
    public string? ImagePath { get; set; }
}
