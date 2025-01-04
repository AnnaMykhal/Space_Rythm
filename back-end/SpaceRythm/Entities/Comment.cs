using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SpaceRythm.Entities;

public class Comment
{
    // Первинний ключ для коментаря
    [Key]
    public int CommentId { get; set; }

    // Зовнішній ключ до таблиці Tracks, яка вказує на трек, до якого залишений коментар
    [ForeignKey("Track")]
    public int TrackId { get; set; }
    public Track Track { get; set; }

    // Зовнішній ключ до таблиці Users, яка вказує на автора коментаря
    [ForeignKey("User")]
    public int UserId { get; set; }
    public User User { get; set; }

    public string Content { get; set; }

    public DateTime PostedDate { get; set; } = DateTime.Now;

    public int LikesCount { get; set; }
}
