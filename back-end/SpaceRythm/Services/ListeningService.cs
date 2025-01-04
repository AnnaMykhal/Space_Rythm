using SpaceRythm.Data;
using SpaceRythm.Entities;
using SpaceRythm.Interfaces;
using Microsoft.EntityFrameworkCore;
using ActiveUp.Net.Security.OpenPGP.Packets;
using Org.BouncyCastle.Asn1.Ocsp;

namespace SpaceRythm.Services;

public class ListeningService : IListeningService
{
    private readonly MyDbContext _context;

    public ListeningService(MyDbContext context)
    {
        _context = context;
    }

    public async Task AddListening(int? userId, int trackId)
    {
        var track = await _context.Tracks.FindAsync(trackId);
        if (track == null)
        {
            throw new ArgumentException($"Track with ID {trackId} not found.");
        }

        var trackMetadata = await _context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
        if (trackMetadata != null)
        {
            trackMetadata.Plays++;
        }
        else
        {
            trackMetadata = new TrackMetadata
            {
                TrackId = trackId,
                Plays = 1,
                Likes = 0,
                CommentsCount = 0
            };
            _context.TrackMetadatas.Add(trackMetadata);
        }
        
        var Userid = userId ?? null;
        var listening = new Listening
        {
            UserId = userId ?? null,
            TrackId = trackId,
            ListenedDate = DateTime.UtcNow
        };

        _context.Listenings.Add(listening);

        if (userId.HasValue && userId != 0)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId.Value);
            if (!userExists)
            {
                throw new ArgumentException($"User with ID {userId} not found.");
            }

            var historyEntry = new UserListeningHistory
            {
                UserId = userId.Value,
                TrackId = trackId,
                Timestamp = DateTime.UtcNow
            };
            _context.UserListeningHistories.Add(historyEntry);
        }
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetListeningsCountForTrack(int trackId)
    {
        return await _context.Listenings.CountAsync(l => l.TrackId == trackId);
    }
}
