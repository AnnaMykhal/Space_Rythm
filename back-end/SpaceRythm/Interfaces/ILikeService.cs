using SpaceRythm.Entities;

namespace SpaceRythm.Interfaces;

public interface ILikeService
{
    Task AddLike(int userId, int trackId);
    Task RemoveLike(int userId, int trackId);
    Task<int> GetLikesCountForTrack(int trackId);
    Task<IEnumerable<Track>> GetLikedTracksByUser(int userId);
}
