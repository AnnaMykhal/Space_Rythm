namespace SpaceRythm.Interfaces;

public interface IListeningService
{
    Task AddListening(int? userId, int trackId);
    Task<int> GetListeningsCountForTrack(int trackId);
}
