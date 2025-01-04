using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceRythm.Entities;

public class PlaylistTracks
{
    [ForeignKey("Playlist")]
    public int PlaylistId { get; set; }

    [ForeignKey("Track")]
    public int TrackId { get; set; }

    public DateTime AddedDate { get; set; }

    public Playlist Playlist { get; set; }
    public Track Track { get; set; }
}