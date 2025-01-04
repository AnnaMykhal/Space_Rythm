using Microsoft.EntityFrameworkCore;
using SpaceRythm.Data;
using SpaceRythm.Entities;
using SpaceRythm.Interfaces;
using SpaceRythm.Models.Comment;


namespace SpaceRythm.Services;
public class CommentService : ICommentService
{
    private readonly MyDbContext _context;

    public CommentService(MyDbContext context)
    {
        _context = context;
    }

    public async Task AddComment(int userId, int trackId, string content)
    {
        var comment = new Comment
        {
            UserId = userId,
            TrackId = trackId,
            Content = content,
            PostedDate = DateTime.Now
        };

        _context.Comments.Add(comment);

        var trackMetadata = await _context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);

        if (trackMetadata != null)
        {
            trackMetadata.CommentsCount++;

        }
        else
        {
            trackMetadata = new TrackMetadata
            {
                TrackId = trackId,
                Plays = 0,
                Likes = 0,
                CommentsCount = 1
            };
            _context.TrackMetadatas.Add(trackMetadata);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Comment> GetCommentByIdAsync(int commentId)
    {
        return await _context.Comments.FindAsync(commentId);
    }

    public async Task<IEnumerable<CommentResponse>> GetCommentsForTrack(int trackId)
    {
        var comments = await _context.Comments
        .Where(c => c.TrackId == trackId)
        .Include(c => c.User)
        .OrderByDescending(c => c.PostedDate)
        .Select(c => new CommentResponse
        {
            CommentId = c.CommentId,
            UserId = c.UserId,
            UserName = c.User.Username,
            Content = c.Content,
            PostedDate = c.PostedDate,
            LikesCount = _context.CommentLikes.Count(cl => cl.CommentId == c.CommentId)
        })
        .ToListAsync();

        return comments;
    }

    public async Task<bool> UpdateComment(int commentId, string content)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
        {
            return false;
        }

        comment.Content = content;
        comment.PostedDate = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task DeleteComment(int commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment != null)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }
    }

}