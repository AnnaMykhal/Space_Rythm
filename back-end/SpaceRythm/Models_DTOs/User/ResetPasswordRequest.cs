using System.ComponentModel.DataAnnotations;

namespace SpaceRythm.Models.User;

public class ResetPasswordRequest
{
    [Required]
    public string Email { get; set; }

    [Required]
    public string Token { get; set; }

    [Required, MinLength(6)]
    public string NewPassword { get; set; }
}
