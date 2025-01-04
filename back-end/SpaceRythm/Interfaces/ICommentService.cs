using SpaceRythm.Entities;
using SpaceRythm.Models.Comment;

namespace SpaceRythm.Interfaces;

public interface ICommentService
{
    Task AddComment(int userId, int trackId, string content);
    Task<IEnumerable<CommentResponse>> GetCommentsForTrack(int trackId);
    Task DeleteComment(int commentId);
    Task<bool> UpdateComment(int commentId, string content);
    Task<Comment> GetCommentByIdAsync(int commentId);
}
