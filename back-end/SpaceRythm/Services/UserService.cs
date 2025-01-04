using SpaceRythm.Entities; 
using SpaceRythm.Interfaces;
using SpaceRythm.Models.User;
using SpaceRythm.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpaceRythm.Models;
using SpaceRythm.Util;
using Microsoft.Extensions.Options;
using SpaceRythm.Exceptions;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Net.Http;
using Microsoft.AspNetCore.Identity.Data;
using ResetPasswordRequest = SpaceRythm.Models.User.ResetPasswordRequest;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Org.BouncyCastle.Ocsp;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using SpaceRythm.Controllers;
using SpaceRythm.Models_DTOs.Role;
using ActiveUp.Net.Security.OpenPGP.Packets;

namespace SpaceRythm.Services
{
    public class UserService : IUserService
    {
        private readonly MyDbContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TokenService _tokenService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<UsersController> _logger;

        public UserService(MyDbContext context, IOptions<JwtSettings> jwtOptions, HttpClient httpClient, TokenService tokenService, IPasswordHasher<User> passwordHasher, IHttpContextAccessor httpContextAccessor, ILogger<UsersController> logger)
        {
            _context = context;
            _jwtSettings = jwtOptions.Value;
            _httpContextAccessor = httpContextAccessor;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAl()
        {
            return await _context.Users.ToListAsync();
        }
        public async Task<IEnumerable<CreateUserResponse>> GetAll()
        {
            var users = await _context.Users.ToListAsync();

            var userResponses = users.Select(user =>
            {
                var avatarUrl = GenerateAvatarUrl(user.Id, user.ProfileImage);
                return new CreateUserResponse(user, null, true, null, avatarUrl);
            });

            return userResponses;
        }

        public async Task<CreateUserResponse> GetById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return null;

            var avatarUrl = GenerateAvatarUrl(user.Id, user.ProfileImage);
            return new CreateUserResponse(user, null, true, null, avatarUrl);
        }

        public async Task<CreateUserResponse> GetByUsername(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return null;

            var avatarUrl = GenerateAvatarUrl(user.Id, user.ProfileImage);
            return new CreateUserResponse(user, null, true, null, avatarUrl);
        }

        public async Task<CreateUserResponse> GetByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return null;

            var avatarUrl = GenerateAvatarUrl(user.Id, user.ProfileImage);
            return new CreateUserResponse(user, null, true, null, avatarUrl);
        }

        private string GenerateAvatarUrl(int userId, string profileImage)
        {
            if (string.IsNullOrEmpty(profileImage))
                return null;

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return null;

            var urlHelperFactory = httpContext.RequestServices.GetService<IUrlHelperFactory>();
            if (urlHelperFactory == null)
                return null;

            var actionContext = new ActionContext(httpContext, httpContext.GetRouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
            var urlHelper = urlHelperFactory.GetUrlHelper(actionContext);

            return urlHelper.Action("GetAvatar", "Users", new { userId }, httpContext.Request.Scheme);
        }

        public async Task<User> GetByMail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<CreateUserResponse> Create(CreateUserRequest req)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (existingUser != null)
            {
                _logger.LogInformation($"User with email '{req.Email}' already exists in the database.");
                return null; 
            }

            if (!IsValidEmail(req.Email))
            {
                throw new ArgumentException("Invalid email format.");
            }
            var user = new User(req);
            user.Password = PasswordHash.Hash(req.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (user.Id > 0)
            {
                Console.WriteLine("!!!if (user.Id > 0)");
                string token = _tokenService.GenerateToken(_jwtSettings, user);

                string emailConfirmationToken = await _tokenService.GenerateEmailConfirmationToken(user.Email);

                return new CreateUserResponse(user, token, false, emailConfirmationToken)
                {
                    IsEmailConfirmed = false,
                    EmailConfirmationToken = emailConfirmationToken
                };
            }
            throw new Exception("User creation failed.");
        }
 
        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            var userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            if (userFromDb == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                userFromDb.IsEmailConfirmed = true;
                _context.Users.Update(userFromDb);
                await _context.SaveChangesAsync();

                return IdentityResult.Success;
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = "Invalid token." });
            }
        }

        public async Task<AuthenticateResponse> Authenticate(AuthenticateRequest req)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user == null)
            {
                throw new Exception("Username or password incorrect");
            }

            if (!user.IsEmailConfirmed)
            {
                throw new Exception("Email not confirmed. Please check your inbox and confirm your email.");
            }

            var isPasswordValid = PasswordHash.Verify(req.Password, user.Password);
            if (!isPasswordValid)
            {
                throw new Exception("Username or password incorrect");
            }

            string token = _tokenService.GenerateToken(_jwtSettings, user);
            _httpContextAccessor.HttpContext.Items["User"] = user;
            return new AuthenticateResponse(user, token);
        }
        public async Task<AuthenticateResponse> AuthenticateWithOAuth(ClaimsPrincipal claimsPrincipal)
        {
            var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email);
            var username = claimsPrincipal.FindFirstValue(ClaimTypes.Name);

            var user = await GetByMail(email);

            if (user == null)
            {
                var createUserRequest = new CreateUserRequest
                {
                    Email = email,
                    Username = username,
                };

                var createUserResponse = await Create(createUserRequest);

                user = new User
                {
                    Id = createUserResponse.Id,
                    Email = createUserResponse.Email,
                    Username = createUserResponse.Username,
                    ProfileImage = createUserResponse.ProfileImage,
                    Biography = createUserResponse.Biography,
                    DateJoined = createUserResponse.DateJoined,
                    IsEmailConfirmed = createUserResponse.IsEmailConfirmed,
                    TracksLiked = new List<TrackLiked>(),
                    ArtistsLiked = new List<ArtistLiked>(),
                    CategoriesLiked = new List<CategoryLiked>()
                };
            }

            var token = _tokenService.GenerateToken(_jwtSettings, user);

            return new AuthenticateResponse(user, token);
        }
        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await GetByMail(email);
            if (user == null)
                return false;

            var isTokenValid = await _tokenService.VerifyPasswordResetToken(email, token);
            if (!isTokenValid)
                return false;

            user.Password = PasswordHash.Hash(newPassword);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<UpdateUserResponse> Update(string id, UpdateUserRequest req)
        {
            if (!int.TryParse(id, out int userId))
            {
                throw new Exception("Invalid user ID");
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            user.Email = req.Email ?? user.Email;
            user.Username = req.Username ?? user.Username;
            user.ProfileImage = req.ProfileImage ?? user.ProfileImage;
            user.Biography = req.Biography ?? user.Biography;

            if (!string.IsNullOrEmpty(req.Password))
            {
                user.Password = PasswordHash.Hash(req.Password);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new UpdateUserResponse{};
        }

        public async Task<string> UploadAvatar(int userId, string avatarPath)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found");

            user.ProfileImage = avatarPath;
            await _context.SaveChangesAsync();

            return user.ProfileImage; 
        }

        public async Task<string> SaveAndUploadAvatarAsync(int userId, IFormFile avatarFile, string wwwRootPath)
        {
            if (avatarFile == null || avatarFile.Length == 0)
            {
                throw new ArgumentException("Invalid avatar file.");
            }

            if (string.IsNullOrEmpty(avatarFile.ContentType) || !avatarFile.ContentType.StartsWith("image/"))
            {
                throw new ArgumentException("File type must be an image.");
            }

            var uploadsFolder = Path.Combine(wwwRootPath, "profileImages");
            Directory.CreateDirectory(uploadsFolder); 

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(avatarFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            var relativePath = $"/profileImages/{fileName}";
            await UploadAvatar(userId, relativePath);

            return GenerateAvatarUrl(userId, relativePath);
        }

        public async Task<string> GetAvatarUrlAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.ProfileImage))
            {
                return string.Empty;
            }
           
            var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host.Value}";
            var avatarUrl = $"{baseUrl}/{user.ProfileImage}";

            return avatarUrl ?? string.Empty; 
        }

        public async Task ChangePassword(int userId, ChangePasswordRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found");

            if (!VerifyPassword(request.OldPassword, user.Password))
            {
                throw new AppException("Old password is incorrect");
            }

            user.Password = HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<bool> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> VerifyFacebookRequest(string accessToken)
        {
            string appId = "526202996837238";
            string url = $"https://graph.facebook.com/me?access_token={accessToken}&fields=id,name,email";

            return null;
        }

        public async Task<bool> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.PasswordResetToken == request.Token);
            if (user == null || user.ResetTokenExpires < DateTime.UtcNow)
            {
                throw new Exception("Invalid or expired reset token");
            }

            user.Password = HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CheckEmailExists(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> CheckUsernameExists(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> ValidatePassword(int userId, string password)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            return passwordVerificationResult == PasswordVerificationResult.Success;
        }

        public async Task AssignRoleAsync(int userId, string roleName)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) throw new Exception("Role not found");

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = role.Id
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsUserInRoleAsync(int userId, string roleName)
        {
            return await _context.UserRoles
                .Include(ur => ur.Role)
                .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == roleName);
        }
    }

    public class FacebookUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

}
   

