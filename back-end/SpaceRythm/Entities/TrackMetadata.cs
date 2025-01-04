using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SpaceRythm.Entities;

public class TrackMetadata
{
    [Key]
    public int MetadataId { get; set; } 

    [ForeignKey("Track")] 
    public int TrackId { get; set; }

    public int Plays { get; set; } 
    public int Likes { get; set; }  
    public int CommentsCount { get; set; }
    [JsonIgnore]
    public Track Track { get; set; }
}


