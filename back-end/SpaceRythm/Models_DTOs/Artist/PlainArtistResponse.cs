using Microsoft.AspNetCore.Mvc;
using SpaceRythm.Models.Track;

namespace SpaceRythm.Models.Artist;

public class PlainArtistResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Bio { get; set; }
    public string ImagePath { get; set; }
    public IEnumerable<PlainTrackResponse> Tracks { get; set; }

    public PlainArtistResponse(Entities.Artist artist, string baseUrl)
    {
        if (artist == null)
            throw new ArgumentNullException(nameof(artist), "Artist cannot be null.");

        Id = artist.ArtistId;
        Name = artist.Name;
        Bio = artist.Bio;

        ImagePath = string.IsNullOrEmpty(artist.ImagePath)
            ? null
            : $"{baseUrl}/artistImages/{artist.ImagePath}";

        Tracks = artist.Tracks?.Select(track =>
            new PlainTrackResponse(track, artist.Name, track.FilePath, track.ImagePath))
            ?? new List<PlainTrackResponse>(); 
    }
}
