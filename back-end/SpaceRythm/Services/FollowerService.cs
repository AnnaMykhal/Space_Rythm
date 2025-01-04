using SpaceRythm.Data;
using SpaceRythm.Entities;
using SpaceRythm.Interfaces;
using Microsoft.EntityFrameworkCore;
using SpaceRythm.Models_DTOs.Follower;

namespace SpaceRythm.Services;

public class FollowerService : IFollowerService
{
    private readonly MyDbContext _context;

    public FollowerService(MyDbContext context)
    {
        _context = context;
    }

    public async Task FollowUserAsync(int userId, int followedUserId)
    {
        if (userId == followedUserId)
            throw new InvalidOperationException("You cannot follow yourself.");

        var alreadyFollowing = await _context.Followers
            .AnyAsync(f => f.UserId == userId && f.FollowedUserId == followedUserId);

        if (alreadyFollowing)
            throw new InvalidOperationException("You are already following this user.");

        var follower = new Follower
        {
            UserId = userId,
            FollowedUserId = followedUserId,
            FollowDate = DateTime.UtcNow
        };

        _context.Followers.Add(follower);
        await _context.SaveChangesAsync();
    }

    public async Task UnfollowUserAsync(int userId, int followedUserId)
    {
        var follower = await _context.Followers
            .FirstOrDefaultAsync(f => f.UserId == userId && f.FollowedUserId == followedUserId);

        if (follower == null)
            throw new InvalidOperationException("You are not following this user.");

        _context.Followers.Remove(follower);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<FollowerResponse>> GetFollowersAsync(int followedUserId)
    {
        var followers = await _context.Followers
            .Where(f => f.FollowedUserId == followedUserId)
            .Select(f => new FollowerResponse
            {
                Id = f.UserId,
                Username = f.User.Username,
                Avatar = f.User.ProfileImage,
                FollowDate = f.FollowDate
            })
            .ToListAsync();

        return followers;
    }

    public async Task<IEnumerable<FollowerResponse>> GetFollowingAsync(int userId)
    {
        var following = await _context.Followers
            .Where(f => f.UserId == userId)
            .Select(f => new FollowerResponse
            {
                Id = f.FollowedUserId,
                Username = f.FollowedUser.Username,
                Avatar = f.FollowedUser.ProfileImage,
                FollowDate = f.FollowDate
            })
            .ToListAsync();

        return following;
    }
}



