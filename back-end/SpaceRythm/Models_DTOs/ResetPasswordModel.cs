using System.ComponentModel.DataAnnotations;

namespace SpaceRythm.Models;

public class ResetPasswordModel
{
    [Required(ErrorMessage = "The new password is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Password confirmation is required.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords don't match")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; }

    public string Email { get; set; }

    public string Token { get; set; }
}