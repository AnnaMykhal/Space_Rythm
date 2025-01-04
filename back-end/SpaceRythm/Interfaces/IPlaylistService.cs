using SpaceRythm.Entities;

namespace SpaceRythm.Interfaces;

public interface IPlaylistService
{
    Task<IEnumerable<Playlist>> GetUserPlaylistsAsync(int userId, int page = 1, int pageSize = 10);
    Task<Playlist> GetPlaylistByIdAsync(int playlistId);
    Task<Playlist> CreatePlaylistAsync(int userId, string name, string description, List<int>? trackIds = null, string? imageUrl = null);
    Task AddTrackToPlaylistAsync(int playlistId, int trackId);
    Task RemoveTrackFromPlaylistAsync(int playlistId, int trackId);
    Task<Playlist> EditPlaylistAsync(int playlistId, string newName, string newDescription, bool? isPublic, string? newImageUrl);
    Task DeletePlaylistAsync(int playlistId);
}
