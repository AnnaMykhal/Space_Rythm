using SpaceRythm.Data;
using SpaceRythm.Entities;
using SpaceRythm.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SpaceRythm.Services;

public class CommentLikeService : ICommentLikeService
{
    private readonly MyDbContext _context;

    public CommentLikeService(MyDbContext context)
    {
        _context = context;
    }

    public async Task AddLike(int userId, int commentId)
    {
        bool alreadyLiked = await _context.CommentLikes
        .AnyAsync(cl => cl.UserId == userId && cl.CommentId == commentId);

        if (alreadyLiked)
        {
            return; 
        }

        var like = new CommentLike
        {
            UserId = userId,
            CommentId = commentId,
            LikedDate = DateTime.Now
        };

        _context.CommentLikes.Add(like);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveLike(int userId, int commentId)
    {
        var like = await _context.CommentLikes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.CommentId == commentId);

        if (like == null)
        {
            return; 
        }

        _context.CommentLikes.Remove(like);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetLikesCountForComment(int commentId)
    {
        return await _context.CommentLikes.CountAsync(l => l.CommentId == commentId);
    }
}
