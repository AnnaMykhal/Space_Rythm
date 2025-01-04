using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SpaceRythm.Entities;

public class AdminLog
{
    [Key]
    public int LogId { get; set; } // Первинний ключ

    [ForeignKey("Admin")]
    public int AdminId { get; set; } // Зовнішній ключ на адміністратора

    [Required]
    [MaxLength(50)]
    public string ActionType { get; set; } // Тип дії (наприклад, блокування, видалення, редагування)

    public int? TargetId { get; set; } // ID користувача або треку (може бути null)

    public DateTime Timestamp { get; set; } // Час виконання дії

    // Навігаційна властивість для адміністратора
    public User Admin { get; set; }
}
