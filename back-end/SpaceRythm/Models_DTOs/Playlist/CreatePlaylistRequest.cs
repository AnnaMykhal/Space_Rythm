namespace SpaceRythm.Models.Playlist;

public class CreatePlaylistRequest
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string? ImageUrl { get; set; }
    public List<int>? TrackIds { get; set; } = new();
}