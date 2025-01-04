using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaceRythm.Entities;
using SpaceRythm.Data;
using SpaceRythm.Attributes;
using SpaceRythm.Interfaces;
using SpaceRythm.Models;
using SpaceRythm.Models.User;
using Org.BouncyCastle.Ocsp;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Identity.Data;
using ResetPasswordRequest = SpaceRythm.Models.User.ResetPasswordRequest;
using ForgotPasswordRequest = SpaceRythm.Models.User.ForgotPasswordRequest;
using Microsoft.AspNetCore.Authorization;
using SpaceRythm.Services;


namespace SpaceRythm.Controllers;


[ApiController]
[Route("api/[controller]")]
public class RegistrationAuthorizationController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly EmailHelper _emailHelper;
    private readonly TokenService _tokenService;

    public RegistrationAuthorizationController(IUserService userService, EmailHelper emailHelper, TokenService tokenService)
    {
        _userService = userService;
        _emailHelper = emailHelper;
        _tokenService = tokenService;
    }

    private bool SendEmail(string recipientEmail, string subject, string message)
    {
        return _emailHelper.SendEmail(recipientEmail, message, subject);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest req)
    {
        if (string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Username) || string.IsNullOrEmpty(req.Password))
        {
            return BadRequest(new { message = "Email, Username, and Password are required" });
        }
        try
        {
            var res = await _userService.Create(req);

            if (res != null && res.Id > 0)
            {
                string emailConfirmationToken = await _tokenService.GenerateEmailConfirmationToken(res.Email);
                string confirmationLink = Url.Action("ConfirmEmail", "RegistrationAuthorization", new { token = emailConfirmationToken, email = res.Email }, Request.Scheme);

                if (SendEmail(res.Email, "Ви успішно зареєстровані в Space Rythm", confirmationLink))
                {
                    return Ok(new { message = "User created successfully. Please confirm your email." });
                }
                else
                {
                    return BadRequest(new { message = "Error occurred while sending confirmation email." });
                }
            }
            else
            {
                return BadRequest(new { message = "User creation failed." });
            }
        }
        catch (Exception e)
        {
            return BadRequest(new { message = $"An error occurred: {e.Message}" });
        }
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string token, string email)
    {
        var user = await _userService.GetByMail(email);
        if (user == null) return BadRequest("Invalid email.");

        var result = await _userService.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            return Ok("Email confirmed successfully.");
        }
        else
        {
            return BadRequest("Error confirming your email.");
        }
    }

    [HttpPost("authenticate")]
    [AllowAnonymous]
    public async Task<IActionResult> Authenticate(AuthenticateRequest req)
    {
        try
        {
            var response = await _userService.Authenticate(req);

            if (response == null)
            {
                ModelState.AddModelError("", "Username or password incorrect");
                return BadRequest(new { message = "Username or password incorrect" });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Email not confirmed"))
            {
                return BadRequest(new { message = ex.Message });
            }

            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("google")]
    public IActionResult GoogleLogin()
    {
        var redirectUrl = Url.Action("GoogleResponse", "Auth", new { }, Request.Scheme);

        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUrl
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse(string redirectUri)
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (result?.Succeeded != true)
        {
            return BadRequest("Authentication failed");
        }

        var authenticateResponse = await _userService.AuthenticateWithOAuth(result.Principal);

        return Redirect($"{redirectUri}?token={authenticateResponse.JwtToken}");
    }

    [HttpGet("facebook")]
    public IActionResult FacebookLogin()
    {
        var redirectUrl = Url.Action("FacebookResponse", "Users", new { }, Request.Scheme);
        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUrl
        };

        return Challenge(properties, FacebookDefaults.AuthenticationScheme);
    }


    [HttpGet("facebook-response")]
    public async Task<IActionResult> FacebookResponse(string redirectUri)
    {
        var result = await HttpContext.AuthenticateAsync(FacebookDefaults.AuthenticationScheme);

        if (result?.Succeeded != true)
        {
            return BadRequest("Authentication failed");
        }

        var authenticateResponse = await _userService.AuthenticateWithOAuth(result.Principal);

        return Redirect($"{redirectUri}?token={authenticateResponse.JwtToken}");
    }


    [HttpPost("delete")]
    public async Task<IActionResult> DeleteFacebookUser([FromBody] FacebookDeletionRequest request)
    {
        var userIdString = await _userService.VerifyFacebookRequest(request.AccessToken);

        if (string.IsNullOrEmpty(userIdString)) 
        {
            return BadRequest(new { message = "Invalid request" });
        }

        if (!int.TryParse(userIdString, out int userId)) 
        {
            return BadRequest(new { message = "Invalid user ID format" });
        }

        await _userService.Delete(userId); 
        return Ok(new { message = "User data deleted successfully" });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromForm] ForgotPasswordRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userService.GetByEmail(model.Email);
        if (user == null)
        {
            return NotFound(new { message = "User with this email does not exist." });
        }

        var token = await _tokenService.GeneratePasswordResetToken(user.Email);
        var url = Url.Action("ResetPassword", "RegistrationAuthorization", new { email = user.Email, token }, Request.Scheme);

        bool result = _emailHelper.SendEmailResetPassword(user.Email, url);
        if (!result)
        {
            return StatusCode(500, new { message = "Error occurred while sending email." });
        }

        return Ok(new { message = "Password reset link sent to email." });
    }

    [HttpGet("forgot-password")]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return Ok(new { message = "Enter your email to reset the password." });
    }

    [HttpGet("forgot-password-confirmation")]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
        return Ok(new { message = "Password reset confirmation page" });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userService.GetByEmail(model.Email);
        if (user == null)
        {
            return NotFound(new { message = "User with this email does not exist." });
        }

        var isValidToken = await _tokenService.VerifyPasswordResetToken(model.Email, model.Token);
        if (!isValidToken)
        {
            return BadRequest(new { message = "Invalid or expired token." });
        }

        var isResetSuccessful = await _userService.ResetPasswordAsync(model.Email, model.Token, model.ConfirmPassword);
        if (!isResetSuccessful)
        {
            return StatusCode(500, new { message = "Error occurred while resetting password." });
        }

        return Ok(new { message = "Password has been successfully reset." });
    }

    [HttpGet("reset-password")]
    [AllowAnonymous]
    public IActionResult ResetPassword(string email, string token)
    {
        return Ok(new { Email = email, Token = token });
    }
}