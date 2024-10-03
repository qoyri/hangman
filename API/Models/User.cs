using HangmanApi.Models;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public ICollection<Score> Scores { get; set; }
    public ProfilePicture ProfilePicture { get; set; } // ProfilPicture pour relation un-à-un
}