using SpaceRythm.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceRythm.Models_DTOs.Role;

public class UserRole
{
    [ForeignKey("User")]
    [Column("user_id")]
    public int UserId { get; set; }
    public User User { get; set; }

    [ForeignKey("Role")]
    [Column("role_id")]
    public int RoleId { get; set; }
    public Role Role { get; set; }
}
