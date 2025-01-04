using static SpaceRythm.Entities.UserCredentials;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SpaceRythm.Models_DTOs.Role;

public class Role
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("role_id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("role_name")]
    public string Name { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}