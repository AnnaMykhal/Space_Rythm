using ActiveUp.Net.Security.OpenPGP.Packets;
using Microsoft.AspNetCore.Mvc;
using SpaceRythm.Data;

namespace SpaceRythm.Models.Track;

public class PlainTrackResponse
{
    public int TrackId { get; set; }
    public string Title { get; set; } 
    public int ArtistId { get; set; }
    public string FilePath { get; set; }
    public Genre Genre { get; set; }
    public List<string> Tags { get; set; }
    public string Description { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime UploadDate { get; set; }
    public int UserId { get; set; }
    public string UploadedBy { get; set; }
    public IEnumerable<int> CategoriesId { get; set; }
    public IEnumerable<string> Categories { get; set; }
    public string ArtistName { get; set; }
    public string ImagePath { get; set; }

    public int Plays { get; set; }
    public int Likes { get; set; }
    public int CommentsCount { get; set; }

    public PlainTrackResponse(Entities.Track track, string artistName, string filePath, string imagePath)
    {
        TrackId = track.TrackId;
        Title = track.Title;
        ArtistId = track.ArtistId;
        FilePath = filePath;
        Genre = (Genre)track.Genre;
        Tags = track.Tags ?? new List<string>();
        Description = track.Description;
        Duration = track.Duration;
        UploadDate = track.UploadDate;
        ArtistName = artistName;
        ImagePath = imagePath;
        UserId = track.UserId;
        UploadedBy = track.User?.Username;
        CategoriesId = track.TrackCategoryLink.Select(link => link.TrackCategoryId).ToList();

        Categories = track.TrackCategoryLink != null && track.TrackCategoryLink.Any()
           ? track.TrackCategoryLink
               .Where(link => link?.TrackCategory != null)
               .Select(link => link.TrackCategory?.Category)
               .Where(category => category != null)
               .ToList()
           : new List<string>();

        Plays = track.TrackMetadata?.Plays ?? 0;
        Likes = track.TrackMetadata?.Likes ?? 0;
        CommentsCount = track.TrackMetadata?.CommentsCount ?? 0;
    }
}
