using SpaceRythm.Data;
using SpaceRythm.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SpaceRythm.Services;

public class UserStatisticsService : IUserStatisticsService
{
    private readonly MyDbContext _context;

    public UserStatisticsService(MyDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetFollowersCount(int userId)
    {
        return await _context.Followers.CountAsync(f => f.FollowedUserId == userId);
    }

    public async Task<int> GetListeningsCount(int userId)
    {
        return await _context.Listenings.CountAsync(l => l.UserId == userId);
    }

    public async Task<int> GetLikesCount(int userId)
    {
        return await _context.Likes.CountAsync(l => l.UserId == userId);
    }

    public async Task<int> GetCommentsCount(int userId)
    {
        return await _context.Comments.CountAsync(c => c.UserId == userId);
    }
}
