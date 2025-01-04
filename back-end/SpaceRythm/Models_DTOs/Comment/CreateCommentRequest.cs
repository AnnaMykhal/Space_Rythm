namespace SpaceRythm.Models.Comment;

public class CreateCommentRequest
{
    public int UserId { get; set; } 
    public int TrackId { get; set; } 
    public string Content { get; set; }
}
