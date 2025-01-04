namespace SpaceRythm.Models.Comment;

public class CommentResponse
{
    public int CommentId { get; set; } 
    public int UserId { get; set; }
    public string UserName { get; set; } 
    public string Content { get; set; } 
    public DateTime PostedDate { get; set; }
    public int LikesCount { get; set; }
}
