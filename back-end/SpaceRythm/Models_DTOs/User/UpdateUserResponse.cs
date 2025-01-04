namespace SpaceRythm.Models.User
{
    public class UpdateUserResponse
    {
        public string Message { get; set; }

        public List<string> Errors { get; set; } = new List<string>();
    }

}
