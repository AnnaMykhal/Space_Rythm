namespace SpaceRythm.Models.User;

public class CreateUserResponse
{
    public int Id { get; set; } 
    public string Email { get; set; }
    public string Username { get; set; }
    public string JwtToken { get; set; }
    public string ProfileImage { get; set; }
    public string Biography { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public DateTime DateJoined { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public string EmailConfirmationToken { get; set; }
    public List<string> TracksLiked { get; set; }
    public List<string> Likes { get; set; }
    public List<string> ArtistsLiked { get; set; }
    public List<string> CategoriesLiked { get; set; }

    public bool Succeeded { get; set; }
    public string AvatarUrl { get; set; }

    public CreateUserResponse(Entities.User user, string token, bool succeeded, string emailConfirmationToken, string avatarUrl = null)
    {
        Id = user.Id; 
        Email = user.Email;
        Username = user.Username;
        JwtToken = token;
        ProfileImage = user.ProfileImage;
        Biography = user.Biography;
        Country = user.Country;
        City = user.City;
        DateJoined = user.DateJoined;
        IsEmailConfirmed = user.IsEmailConfirmed;
        EmailConfirmationToken = emailConfirmationToken;
        Succeeded = succeeded;
        AvatarUrl = avatarUrl;

        TracksLiked = user.Likes.Select(l => l.Track.TrackId.ToString()).ToList();
        Likes = user.Likes.Select(s => s.TrackId.ToString()).ToList();
        ArtistsLiked = user.ArtistsLiked.Select(a => a.ArtistId.ToString()).ToList();
        CategoriesLiked = user.CategoriesLiked.Select(c => c.CategoryId.ToString()).ToList();
    }

    
}