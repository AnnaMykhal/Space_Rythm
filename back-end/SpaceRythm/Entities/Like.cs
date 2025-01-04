using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SpaceRythm.Entities;

public class Like
{
    // Композитний ключ
    [Key, Column(Order = 0)]
    public int UserId { get; set; } // Ідентифікатор користувача, який поставив лайк

    [Key, Column(Order = 1)]
    public int TrackId { get; set; }

    public DateTime LikedDate { get; set; }

    // Навігаційні властивості
    public User User { get; set; }
    public Track Track { get; set; }
}