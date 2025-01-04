using SpaceRythm.Data;
using SpaceRythm.Entities;
using SpaceRythm.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace SpaceRythm.Services;

public class LikeService : ILikeService
{
    private readonly MyDbContext _context;

    public LikeService(MyDbContext context)
    {
        _context = context;
    }

    public async Task AddLike(int userId, int trackId)
    {
        var existingLike = await _context.Likes
            .FirstOrDefaultAsync(like => like.UserId == userId && like.TrackId == trackId);

        if (existingLike != null)
        {
            return; 
        }

        var like = new Like
        {
            UserId = userId,
            TrackId = trackId,
            LikedDate = DateTime.Now
        };

        _context.Likes.Add(like);

        var trackMetadata = await _context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);

        if (trackMetadata != null)
        {
            trackMetadata.Likes++;
        }
        else
        {
            trackMetadata = new TrackMetadata
            {
                TrackId = trackId,
                Plays = 0,
                Likes = 1,
                CommentsCount = 0
            };

            _context.TrackMetadatas.Add(trackMetadata);
        }

        var track = await _context.Tracks
            .Include(t => t.TrackCategoryLink)
            .ThenInclude(link => link.TrackCategory)
            .Include(t => t.Artist)
            .FirstOrDefaultAsync(t => t.TrackId == trackId);

        if (track != null)
        {
            if (track.TrackCategoryLink != null && track.TrackCategoryLink.Any())
            {
                var trackCategoryLink = track.TrackCategoryLink.FirstOrDefault();
                if (trackCategoryLink != null && trackCategoryLink.TrackCategory != null)
                {
                    var categoryLikedExists = await _context.CategoriesLiked
                        .AnyAsync(cl => cl.UserId == userId && cl.CategoryId == trackCategoryLink.TrackCategory.Id);

                    if (!categoryLikedExists)
                    {
                        var categoryLiked = new CategoryLiked
                        {
                            UserId = userId,
                            CategoryId = trackCategoryLink.TrackCategory.Id
                        };

                        _context.CategoriesLiked.Add(categoryLiked);
                    }
                }
            }

            if (track.Artist != null)
            {
                var artistLikedExists = await _context.ArtistsLiked
                    .AnyAsync(al => al.UserId == userId && al.ArtistId == track.Artist.ArtistId);

                if (!artistLikedExists)
                {
                    var artistLiked = new ArtistLiked
                    {
                        UserId = userId,
                        ArtistId = track.Artist.ArtistId
                    };

                    _context.ArtistsLiked.Add(artistLiked);
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task RemoveLike(int userId, int trackId)
    {
        var like = await _context.Likes.FirstOrDefaultAsync(l => l.UserId == userId && l.TrackId == trackId);
        if (like == null)
        {
            return;
        }

        _context.Likes.Remove(like);

        var trackMetadata = await _context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
        if (trackMetadata != null && trackMetadata.Likes > 0)
        {
            trackMetadata.Likes--;
        }

        var track = await _context.Tracks
            .Include(t => t.TrackCategoryLink)
            .ThenInclude(link => link.TrackCategory)
            .Include(t => t.Artist)
            .FirstOrDefaultAsync(t => t.TrackId == trackId);

        if (track != null)
        {
            if (track.TrackCategoryLink != null && track.TrackCategoryLink.Any())
            {
                foreach (var trackCategoryLink in track.TrackCategoryLink)
                {
                    if (trackCategoryLink.TrackCategory != null)
                    {
                        var isCategoryStillLiked = await _context.Likes
                            .AnyAsync(l => l.UserId == userId &&
                                           l.Track.TrackCategoryLink
                                               .Any(tcl => tcl.TrackCategoryId == trackCategoryLink.TrackCategoryId));

                        if (!isCategoryStillLiked)
                        {
                            var categoryLiked = await _context.CategoriesLiked
                                .FirstOrDefaultAsync(cl => cl.UserId == userId && cl.CategoryId == trackCategoryLink.TrackCategoryId);

                            if (categoryLiked != null)
                            {
                                _context.CategoriesLiked.Remove(categoryLiked);
                            }
                        }
                    }
                }
            }

            if (track.Artist != null)
            {
                var isArtistStillLiked = await _context.Likes
                    .AnyAsync(l => l.UserId == userId && l.Track.Artist.ArtistId == track.Artist.ArtistId);

                if (!isArtistStillLiked)
                {
                    var artistLiked = await _context.ArtistsLiked
                        .FirstOrDefaultAsync(al => al.UserId == userId && al.ArtistId == track.Artist.ArtistId);

                    if (artistLiked != null)
                    {
                        _context.ArtistsLiked.Remove(artistLiked);
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetLikesCountForTrack(int trackId)
    {
        return await _context.Likes.CountAsync(l => l.TrackId == trackId);
    }

    public async Task<IEnumerable<Track>> GetLikedTracksByUser(int userId)
    {
        var likedTracks = await _context.Likes
            .Where(l => l.UserId == userId)
            .Include(l => l.Track)
            .Select(l => l.Track)
            .ToListAsync();

        return likedTracks;
    }
}
