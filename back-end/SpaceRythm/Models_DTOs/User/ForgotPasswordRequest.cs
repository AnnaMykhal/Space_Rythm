using System.ComponentModel.DataAnnotations;

namespace SpaceRythm.Models.User;

public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; }
}
