namespace SpaceRythm.Interfaces;

public interface IUserStatisticsService
{
    Task<int> GetFollowersCount(int userId);
    Task<int> GetListeningsCount(int userId);
    Task<int> GetLikesCount(int userId);
    Task<int> GetCommentsCount(int userId);
}
