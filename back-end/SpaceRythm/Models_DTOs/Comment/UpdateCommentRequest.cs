namespace SpaceRythm.Models.Comment;

public class UpdateCommentRequest
{
    public int CommentId { get; set; } 
    public string Content { get; set; }
    public int UserId {  get; set; }
}
