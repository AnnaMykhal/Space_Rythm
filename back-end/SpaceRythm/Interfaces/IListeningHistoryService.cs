using SpaceRythm.Entities;

namespace SpaceRythm.Interfaces;

public interface IListeningHistoryService
{
    Task AddToListeningHistory(int userId, int trackId);
    Task<IEnumerable<object>> GetListeningHistory(int userId, int? limit = null);
}