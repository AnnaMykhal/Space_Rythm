using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaceRythm.Entities;
using SpaceRythm.Data;
using SpaceRythm.Interfaces;
using SpaceRythm.Models;
using SpaceRythm.Models.User;
using SpaceRythm.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace SpaceRythm.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly IUserService _userService;
    private readonly MyDbContext _context;
    private readonly IUserStatisticsService _userStatisticsService;
    private readonly IWebHostEnvironment _environment;

    public UsersController(IUserService userService, IUserStatisticsService userStatisticsService, MyDbContext context, ILogger<UsersController> logger, IWebHostEnvironment environment)
    {
        _userService = userService;
        _userStatisticsService = userStatisticsService;
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    // Get users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CreateUserResponse>>> GetAllUsers()
    {
        var users = await _userService.GetAll();
        return Ok(users);
    }

    // Отримати конкретного user по id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var userResponse = await _userService.GetById(id);
        if (userResponse == null)
            return NotFound(new { message = "User not found" });

        return Ok(userResponse);
    }

    // Отримати конкретного user по username
    [HttpGet("by-username/{username}")]
    public async Task<IActionResult> GetByUsername(string username)
    {
        var userResponse = await _userService.GetByUsername(username);
        if (userResponse == null)
            return NotFound(new { message = "User not found" });

        return Ok(userResponse);
    }

    // Отримати конкретного user по email

    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var userResponse = await _userService.GetByEmail(email);

        if (userResponse == null)
            return NotFound(new { message = "User not found" });

        return Ok(userResponse);
    }

    [HttpGet("get-avatar")]
    public IActionResult GetAvatar(int userId)
    {
        var user = _context.Users.Find(userId);
        if (user == null || string.IsNullOrEmpty(user.ProfileImage))
        {
            return NotFound("Avatar not found");
        }

        var avatarPath = Path.Combine(_environment.WebRootPath, user.ProfileImage.TrimStart('/'));

        if (!System.IO.File.Exists(avatarPath))
        {
            return NotFound("Avatar file does not exist.");
        }

        var fileExtension = Path.GetExtension(avatarPath).ToLower();
        string mimeType = fileExtension switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };

        return PhysicalFile(avatarPath, mimeType);
    }
   

    [HttpGet("get-avatar-url")]
    public async Task<IActionResult> GetAvatarUrl([FromQuery] string userId)
    {
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
        {
            return BadRequest("Invalid or missing 'userId' parameter.");
        }

        var avatarUrl = await _userService.GetAvatarUrlAsync(id); ;
        if (avatarUrl == null)
        {
            return NotFound("Avatar not found for the specified user.");
        }

        return Ok(new { avatarUrl });
    }


    // Завантаження профілю зображення
    [Authorize]
    [HttpPost("upload-avatar")]
    public async Task<IActionResult> UploadAvatar([FromForm] IFormFile avatar)
    {
        _logger.LogInformation("Початок методу UploadAvatar");

        if (!User.Identity.IsAuthenticated)
        {
            _logger.LogWarning("User is not authenticated.");
            return Unauthorized(new { message = "User not authorized" });
        }

        if (!int.TryParse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value, out int userId))
        {
            return Unauthorized(new { message = "User ID not found in claims" });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BadRequest(new { message = "User not found" });

        if (avatar == null || avatar.Length == 0)
            return BadRequest(new { message = "Invalid avatar file" });

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profileImages");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileExtension = Path.GetExtension(avatar.FileName).ToLower();
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var relativePath = $"/profileImages/{uniqueFileName}"; 
        var absolutePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(absolutePath, FileMode.Create))
        {
            await avatar.CopyToAsync(stream);
        }

        var avatarPath = await _userService.UploadAvatar(user.Id, relativePath);

        return Ok(new { avatarPath });
    }


    // Update інформації користувача
    [Authorize]
    [HttpPut("update")]
    public async Task<IActionResult> Update(UpdateUserRequest req)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userService.GetById(Convert.ToInt32(userId));

        if (userId == null)
            return BadRequest(new { message = "User not found" });

        var res = await _userService.Update(userId.ToString(), req);
        return Ok(res);
    }

    // Зміна пароля користувача
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest req)
    {
        var user = HttpContext.Items["User"] as User;

        if (user == null)
            return BadRequest(new { message = "User not found" });

        if (!await UserHasAccessToTrack(user.Id))
        {
            return Forbid();
        }

        await _userService.ChangePassword(user.Id, req);
        return Ok(new { message = "Password successfully changed." });
    }

    // Delete поточного користувача
    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        var user = HttpContext.Items["User"] as User;

        if (user == null)
            return BadRequest(new { message = "User not found" });

        await _userService.Delete(user.Id);
        return Ok();
    }

    //Видалення користувача по Id
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return BadRequest(new { message = "User not found" });

        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        if (user.Id.ToString() != userId && !isAdmin)
        {
            return Unauthorized("You are not authorized to delete this user.");
        }

        var result = await _userService.Delete(id);
        if (!result)
            return NotFound(new { message = "User not found" });

        return Ok(new { message = "User deleted successfully" });
    }

    [Authorize]
    [HttpGet("{userId}/followers-count")]
    public async Task<IActionResult> GetFollowersCount(int userId)
    {
        var count = await _userStatisticsService.GetFollowersCount(userId);
        return Ok(new { FollowersCount = count });
    }

    [Authorize]
    [HttpGet("{userId}/listenings-count")]
    public async Task<IActionResult> GetListeningsCount(int userId)
    {
        var count = await _userStatisticsService.GetListeningsCount(userId);
        return Ok(new { ListeningsCount = count });
    }

    [Authorize]
    [HttpGet("{userId}/likes-count")]
    public async Task<IActionResult> GetLikesCount(int userId)
    {
        var count = await _userStatisticsService.GetLikesCount(userId);
        return Ok(new { LikesCount = count });
    }

    [Authorize]
    [HttpGet("{userId}/comments-count")]
    public async Task<IActionResult> GetCommentsCount(int userId)
    {
        var count = await _userStatisticsService.GetCommentsCount(userId);
        return Ok(new { CommentsCount = count });
    }

    [HttpGet("{userId}/playlists-count")]
    public async Task<IActionResult> GetUserPlaylistCount(int userId)
    {
        var count = await _context.Playlists.CountAsync(p => p.UserId == userId);
        return Ok(new { PlaylistCount = count });
    }

    [HttpGet("exists/{email}")]
    public async Task<IActionResult> CheckAccountExists(string email)
    {
        var userExists = await _userService.CheckEmailExists(email);
        return Ok(new { Exists = userExists });
    }

    [Authorize]
    [HttpPost("validate-password")]
    public async Task<IActionResult> ValidatePassword([FromBody] string password)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            return Unauthorized(new { message = "User ID not found or invalid." });

        var isValid = await _userService.ValidatePassword(userId, password);
        return Ok(new { IsValid = isValid });
    }

    [HttpGet("username-exists/{username}")]
    public async Task<IActionResult> CheckUsernameExists(string username)
    {
        var usernameExists = await _userService.CheckUsernameExists(username);
        return Ok(new { Exists = usernameExists });
    }

    [HttpGet("email-exists/{email}")]
    public async Task<IActionResult> CheckEmailExists(string email)
    {
        var emailExists = await _userService.CheckEmailExists(email);
        return Ok(new { Exists = emailExists });
    }

    private string? GetCurrentUserId()
    {
        return User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    // Перевірка прав доступу
    private async Task<bool> UserHasAccessToTrack(int userId)
    {
        var user = await _userService.GetById(userId);
        if (user == null)
        {
            return false;
        }

        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");

        return isAdmin || (user.Id).ToString() == currentUserId;
    }
}

