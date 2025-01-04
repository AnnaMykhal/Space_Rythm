using System.ComponentModel.DataAnnotations;

namespace SpaceRythm.Models.User;

public class AuthenticateRequest
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }

}
