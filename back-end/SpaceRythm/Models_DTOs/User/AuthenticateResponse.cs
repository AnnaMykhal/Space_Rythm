namespace SpaceRythm.Models.User;

public class AuthenticateResponse
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string JwtToken { get; set; }
    public string ProfileImage { get; set; }
    public string Biography { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public DateTime DateJoined { get; set; }
    public bool IsEmailConfirmed { get; set; }

    public List<string> SongsLiked { get; set; }
    public List<string> ArtistsLiked { get; set; }
    public List<string> CategoriesLiked { get; set; }

    public AuthenticateResponse(Entities.User user, string token)
    {
        Id = user.Id;
        Username = user.Username;
        Email = user.Email;
        JwtToken = token;
        ProfileImage = user.ProfileImage;
        Biography = user.Biography;
        Country = user.Country;
        City = user.City;
        DateJoined = user.DateJoined;
        IsEmailConfirmed = user.IsEmailConfirmed;

        SongsLiked = user.TracksLiked.Select(s => s.TrackId.ToString()).ToList();
        ArtistsLiked = user.ArtistsLiked.Select(a => a.ArtistId.ToString()).ToList();
        CategoriesLiked = user.CategoriesLiked.Select(c => c.CategoryId.ToString()).ToList();
    }
}