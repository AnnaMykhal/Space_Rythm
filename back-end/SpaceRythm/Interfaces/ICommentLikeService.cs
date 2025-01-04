namespace SpaceRythm.Interfaces;

public interface ICommentLikeService
{
    Task AddLike(int userId, int commentId);
    Task RemoveLike(int userId, int commentId);
    Task<int> GetLikesCountForComment(int commentId);
}
