using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ActiveUp.Net.Groupware.vCard;
using SpaceRythm.Models.Artist;
using System.Diagnostics;
using SpaceRythm.Models.Track;
using SpaceRythm.Data;
using System.Text.Json.Serialization;

namespace SpaceRythm.Entities;

public class Track
{
    [Key] 
    public int TrackId { get; set; }

    [Required] 
    public string Title { get; set; }

    public Genre? Genre { get; set; }
    [JsonIgnore]
    public List<string>? Tags { get; set; }
    //public string Tags { get; set; }
    public string? Description { get; set; }
    public string FilePath { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime UploadDate { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }

    [ForeignKey("Artist")] 
    public int ArtistId { get; set; }
    [JsonIgnore]
    public Artist Artist { get; set; }
    public string? ImagePath { get; set; }
    public TrackMetadata TrackMetadata { get; set; }
    [JsonIgnore]
    public ICollection<TrackCategoryLink> TrackCategoryLink { get; set; } = new List<TrackCategoryLink>();
    public ICollection<TrackLiked> TracksLiked { get; set; } = new List<TrackLiked>();
    
    [JsonIgnore]
    public ICollection<PlaylistTracks> PlaylistTracks { get; set; }
    [JsonIgnore]
    public ICollection<Like> Likes { get; set; } = new List<Like>();
 
    //public ICollection<Listening> Listenings { get; set; } = new List<Listening>();
    public Track(CreateTrackRequest req, MyDbContext context)
    {
        Title = req.Title;
        Genre = req.Genre;
        Tags = req.Tags;
        Description = req.Description;
        //ImagePath = req.ImagePath;
        Duration = TimeSpan.Zero;  
        UploadDate = DateTime.Now;

        var artist = context.Artists.FirstOrDefault(a => a.Name == req.ArtistName);
        if (artist == null)
        {
            artist = new Artist { Name = req.ArtistName };
            context.Artists.Add(artist);
            context.SaveChanges(); 
        }

        ArtistId = artist.ArtistId;

        TracksLiked = new List<TrackLiked>();
        PlaylistTracks = new List<PlaylistTracks>();
        Likes = new List<Like>();
        //Listenings = new List<Listening>();
        // Обробка категорій
        TrackCategoryLink = new List<TrackCategoryLink>();
        foreach (var categoryName in req.Categories)
        {
            var category = context.TrackCategories.FirstOrDefault(c => c.Category == categoryName);
            if (category == null)
            {
                category = new TrackCategory { Category = categoryName };
                context.TrackCategories.Add(category);
                context.SaveChanges(); 
            }

            // Додаємо зв'язок між треком та категорією
            TrackCategoryLink.Add(new TrackCategoryLink { TrackCategoryId = category.Id });
        }

    }

    // Порожній конструктор для ORM
    public Track() { }
}
