using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using SpaceRythm.Entities;
using SpaceRythm.Models.User;
using System.Security.Claims;
using ResetPasswordRequest = SpaceRythm.Models.User.ResetPasswordRequest;



namespace SpaceRythm.Interfaces;

public interface IUserService
{
    //Task<IEnumerable<User>> GetAll();
    Task<IEnumerable<CreateUserResponse>> GetAll();
    Task<IEnumerable<User>> GetAl();
    Task<CreateUserResponse> GetById(int id);

    Task<CreateUserResponse> GetByUsername(string username);
    Task<CreateUserResponse> GetByEmail(string email);

    Task<User> GetByMail(string email);

    Task<CreateUserResponse> Create(CreateUserRequest req);

    Task<AuthenticateResponse> Authenticate(AuthenticateRequest req);
    Task<AuthenticateResponse> AuthenticateWithOAuth(ClaimsPrincipal claimsPrincipal);
    Task<UpdateUserResponse> Update(string id, UpdateUserRequest req);

    Task<string> UploadAvatar(int userId, string avatarPath);
    Task<string> GetAvatarUrlAsync(int userId);
    //Task<string?> GetUserAvatarNameAsync(int userId);
    //Task FollowUser(int followerId, int followeeId);
    //Task<IEnumerable<FollowerDto>> GetFollowers(int userId);
    Task ChangePassword(int userId, ChangePasswordRequest request);
    //Task<bool> Delete(string id);
    Task <bool> Delete(int id);
    //Task<bool> VerifyFacebookRequest(string accessToken);
    Task<string> VerifyFacebookRequest(string accessToken);
    Task<IdentityResult> ConfirmEmailAsync(User user, string token);
    Task<bool> ResetPassword(ResetPasswordRequest request);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    Task<bool> CheckEmailExists(string email);
    Task<bool> CheckUsernameExists(string username);
    Task<bool> ValidatePassword(int userId, string password);
    Task AssignRoleAsync(int userId, string roleName);
    Task<bool> IsUserInRoleAsync(int userId, string roleName);
    Task<string> SaveAndUploadAvatarAsync(int userId, IFormFile avatarFile, string wwwRootPath);
}

