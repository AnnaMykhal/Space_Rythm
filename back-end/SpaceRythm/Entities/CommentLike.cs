using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceRythm.Entities;

public class CommentLike
{
    public int UserId { get; set; } 
    public int CommentId { get; set; } 

    public DateTime LikedDate { get; set; }

    public User User { get; set; }
    public Comment Comment { get; set; }
}