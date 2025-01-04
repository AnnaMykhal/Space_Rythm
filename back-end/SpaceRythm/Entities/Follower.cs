using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SpaceRythm.Entities;

public class Follower
{
    [Key, Column(Order = 0)]
    public int UserId { get; set; } // Користувач, який підписується

    [Key, Column(Order = 1)]
    public int FollowedUserId { get; set; } // Користувач, на якого підписані

    public DateTime FollowDate { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } // Навігаційна властивість до підписника

    [ForeignKey("FollowedUserId")]
    public User FollowedUser { get; set; } // Навігаційна властивість до того, на кого підписані
}