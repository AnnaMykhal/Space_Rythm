using SpaceRythm.Data;
using SpaceRythm.Entities;
using SpaceRythm.Interfaces;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace SpaceRythm.Services;

public class ListeningHistoryService : IListeningHistoryService
{
    private readonly MyDbContext _context;

    public ListeningHistoryService(MyDbContext context)
    {
        _context = context;
    }

    public async Task AddToListeningHistory(int userId, int trackId)
    {
        var historyEntry = new UserListeningHistory
        {
            UserId = userId,
            TrackId = trackId,
            Timestamp = DateTime.UtcNow
        };

        _context.UserListeningHistories.Add(historyEntry);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<object>> GetListeningHistory(int userId, int? limit = null)
    {
        var query = _context.UserListeningHistories
       .Where(h => h.UserId == userId)
       .Include(h => h.Track)
           .ThenInclude(t => t.Artist)
       .OrderByDescending(h => h.Timestamp);

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<UserListeningHistory>)query
                        .Take(limit.Value);
        }

        var history = await query.ToListAsync();

        var result = history.Select(h => new
        {
            h.Id, 
            h.UserId, 
            h.TrackId, 
            h.Timestamp,
            Track = new
            {
                h.Track.TrackId,
                h.Track.Title,
                h.Track.Genre,
                h.Track.Description,
                TrackUrl = $"https://localhost:5017/api/Tracks/file/{h.Track.TrackId}", 
                ImageUrl = $"https://localhost:5017/api/Tracks/image/{h.Track.TrackId}", 
                h.Track.Duration,
                h.Track.UploadDate,
                h.Track.UserId,
                h.Track.ArtistId,
                ArtistName = h.Track.Artist?.Name ?? "Unknown Artist" 
            }
        });

        return result;
    }
}
