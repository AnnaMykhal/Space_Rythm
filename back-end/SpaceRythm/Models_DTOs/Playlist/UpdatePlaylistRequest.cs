namespace SpaceRythm.Models.Playlist;

public class UpdatePlaylistRequest
{
    public string NewName { get; set; }
    public string NewDescription { get; set; }
    public bool? IsPublic { get; set; }
    public IFormFile? NewFile { get; set; }
}
