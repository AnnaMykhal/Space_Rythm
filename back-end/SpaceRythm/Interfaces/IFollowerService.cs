using SpaceRythm.Entities;
using SpaceRythm.Models_DTOs.Follower;

namespace SpaceRythm.Interfaces;

public interface IFollowerService
{
    Task FollowUserAsync(int userId, int followedUserId);
    Task UnfollowUserAsync(int userId, int followedUserId);
    Task<IEnumerable<FollowerResponse>> GetFollowersAsync(int followedUserId);
    Task<IEnumerable<FollowerResponse>> GetFollowingAsync(int userId);
}
