using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceRythm.Entities;
using SpaceRythm.Interfaces;
using SpaceRythm.Services;

namespace SpaceRythm.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FollowersController : ControllerBase
{
    private readonly IFollowerService _followerService;
    public FollowersController(IFollowerService followerService)
    {
        _followerService = followerService;
    }

    // Підписатися
    [Authorize]
    [HttpPost("{userId}/follow/{followedUserId}")]
    public async Task<IActionResult> FollowUser(int userId, int followedUserId)
    {
        try
        {
            await _followerService.FollowUserAsync(userId, followedUserId);
            return Ok(new { message = "Followed successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Відписатися
    [Authorize]
    [HttpDelete("{userId}/unfollow/{followedUserId}")]
    public async Task<IActionResult> UnfollowUser(int userId, int followedUserId)
    {
        try
        {
            await _followerService.UnfollowUserAsync(userId, followedUserId);
            return Ok(new { message = "Unfollowed successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Отримати список підписників
    [HttpGet("{userId}/followers")]
    public async Task<IActionResult> GetFollowers(int userId)
    {
        var followers = await _followerService.GetFollowersAsync(userId);
        return Ok(followers);
    }

    // Отримати список підписок
    [HttpGet("{userId}/following")]
    public async Task<IActionResult> GetFollowing(int userId)
    {
        var following = await _followerService.GetFollowingAsync(userId);
        return Ok(following);
    }
}
