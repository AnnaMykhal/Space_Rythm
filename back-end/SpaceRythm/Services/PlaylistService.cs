using Microsoft.EntityFrameworkCore;
using SpaceRythm.Data;
using SpaceRythm.Entities;
using SpaceRythm.Interfaces;

namespace SpaceRythm.Services;

public class PlaylistService : IPlaylistService
{
    private readonly MyDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PlaylistService(MyDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<Playlist>> GetUserPlaylistsAsync(int userId, int page = 1, int pageSize = 10)
    {
        var playlists = await _context.Playlists
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.PlaylistTracks)
                .ThenInclude(pt => pt.Track)
            .ToListAsync();

        var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host.Value}";

        foreach (var playlist in playlists)
        {
            if (!string.IsNullOrEmpty(playlist.ImageUrl))
            {
                playlist.ImageUrl = $"{baseUrl}/{playlist.ImageUrl}";
            }
        }

        return playlists;
    }

    public async Task<Playlist> GetPlaylistByIdAsync(int playlistId)
    {
        var playlist = await _context.Playlists
            .Include(p => p.PlaylistTracks)
                .ThenInclude(pt => pt.Track)
            .FirstOrDefaultAsync(p => p.PlaylistId == playlistId);

        if (playlist == null)
        {
            throw new Exception("Playlist not found.");
        }

        var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host.Value}";

        if (!string.IsNullOrEmpty(playlist.ImageUrl))
        {
            playlist.ImageUrl = $"{baseUrl}/{playlist.ImageUrl}";
        }

        return playlist;
    }

    public async Task<Playlist> CreatePlaylistAsync(int userId, string name, string description, List<int>? trackIds = null, string? imageUrl = null)
    {
        var existingPlaylist = await _context.Playlists
        .FirstOrDefaultAsync(p => p.UserId == userId && p.Title == name);

        if (existingPlaylist != null)
        {
            throw new Exception("A playlist with this title already exists for the user.");
        }

        if (trackIds != null && trackIds.Any())
        {
            var existingTracks = await _context.Tracks
                .Where(t => trackIds.Contains(t.TrackId))
                .ToListAsync();

            if (existingTracks.Count != trackIds.Count)
                throw new Exception("One or more tracks do not exist.");
        }

        var playlist = new Playlist
        {
            UserId = userId,
            Title = name,
            Description = description,
            CreatedDate = DateTime.Now,
            IsPublic = true,
            ImageUrl = imageUrl,
            PlaylistTracks = trackIds?.Select(trackId => new PlaylistTracks
            {
                TrackId = trackId,
                AddedDate = DateTime.Now
            }).ToList() ?? new List<PlaylistTracks>()
        };

        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync();

        return playlist;
    }

    public async Task AddTrackToPlaylistAsync(int playlistId, int trackId)
    {
        var playlist = await _context.Playlists
            .Include(p => p.PlaylistTracks)
            .FirstOrDefaultAsync(p => p.PlaylistId == playlistId);

        if (playlist == null)
            throw new KeyNotFoundException("Playlist not found.");

        if (playlist.PlaylistTracks.Any(pt => pt.TrackId == trackId))
            throw new Exception("Track already exists in the playlist.");

        var playlistTrack = new PlaylistTracks
        {
            PlaylistId = playlistId,
            TrackId = trackId,
            AddedDate = DateTime.Now
        };

        playlist.PlaylistTracks.Add(playlistTrack);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveTrackFromPlaylistAsync(int playlistId, int trackId)
    {
        var playlistTrack = await _context.PlaylistTracks
            .FirstOrDefaultAsync(pt => pt.PlaylistId == playlistId && pt.TrackId == trackId);

        if (playlistTrack == null)
            throw new KeyNotFoundException("Track not found in the playlist.");

        _context.PlaylistTracks.Remove(playlistTrack);
        await _context.SaveChangesAsync();
    }

    public async Task<Playlist> EditPlaylistAsync(int playlistId, string newName, string newDescription, bool? isPublic, string? newImageUrl)
    {
        var playlist = await _context.Playlists.FirstOrDefaultAsync(p => p.PlaylistId == playlistId);

        if (playlist == null)
        {
            throw new Exception("Playlist not found");
        }

        playlist.Title = newName;
        playlist.Description = newDescription;

        if (isPublic.HasValue) playlist.IsPublic = isPublic.Value;

        if (!string.IsNullOrEmpty(newImageUrl))
            playlist.ImageUrl = newImageUrl;

        await _context.SaveChangesAsync();

        return playlist;
    }

    public async Task DeletePlaylistAsync(int playlistId)
    {
        var playlist = await _context.Playlists
            .Include(p => p.PlaylistTracks)
            .FirstOrDefaultAsync(p => p.PlaylistId == playlistId);

        if (playlist == null)
            throw new KeyNotFoundException("Playlist not found.");

        _context.PlaylistTracks.RemoveRange(playlist.PlaylistTracks);
        _context.Playlists.Remove(playlist);

        await _context.SaveChangesAsync();
    }
}