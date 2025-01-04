using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SpaceRythm.Models.User;
using SpaceRythm.Util;
using SpaceRythm.Models_DTOs.Role;


namespace SpaceRythm.Entities;

public class User 
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("user_id")] 
    public int Id { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [MaxLength(255)]
    [Column("email")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [MaxLength(255)]
    [Column("password_hash")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Username is required.")]
    [MaxLength(100)]
    [Column("username")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
    public string Username { get; set; }
    public bool IsAdmin { get; set; } = false;
    [MaxLength(255)]
    [Column("profile_image")] 
    public string? ProfileImage { get; set; }

    [Column("biography", TypeName = "TEXT")] 
    public string? Biography { get; set; }

    [Column("date_joined")]
    public DateTime DateJoined { get; set; } = DateTime.UtcNow; 

    [MaxLength(50)]
    [Column("oauth_provider")]
    public string? OAuthProvider { get; set; }

    [Column("is_email_confirmed")]
    public bool IsEmailConfirmed { get; set; } = false; // Значення за замовчуванням

    [MaxLength(255)]
    [Column("password_reset_token")]
    public string? PasswordResetToken { get; set; }

    [Column("reset_token_expires")]
    public DateTime? ResetTokenExpires { get; set; }

    [MaxLength(100)]
    [Column("country")]
    public string Country { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("city")]
    public string City { get; set; } = string.Empty;

    // Навігаційні властивості
    public List<TrackLiked> TracksLiked { get; set; } = new List<TrackLiked>();
    public List<ArtistLiked> ArtistsLiked { get; set; } = new List<ArtistLiked>();
    public List<CategoryLiked> CategoriesLiked { get; set; } = new List<CategoryLiked>();

    public ICollection<Track> Tracks { get; set; }
    public ICollection<Playlist> Playlists { get; set; }
    public ICollection<Follower> Followers { get; set; }
    public ICollection<Follower> FollowedBy { get; set; }
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public User(CreateUserRequest req)
    {
        Email = req.Email;
        Username = req.Username;
        Password = PasswordHash.Hash(req.Password);
        DateJoined = DateTime.UtcNow; // Установіть дату при створенні користувача
    }

    // Порожній конструктор, необхідний для ORM
    public User() { }
}
