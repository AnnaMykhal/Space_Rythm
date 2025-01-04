using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SpaceRythm.Entities;

public class Subscription
{
    [Key]
    public int SubscriptionId { get; set; } 
    [ForeignKey("User")]
    public int UserId { get; set; } 

    public SubscriptionType Type { get; set; } // Тип підписки (Free, Premium)

    public DateTime SubscriptionStartDate { get; set; } 

    public DateTime? SubscriptionEndDate { get; set; } 

    // Навігаційна властивість
    public User User { get; set; }
}

// Перелічення для типів підписки
public enum SubscriptionType
{
    Free,
    Premium
}