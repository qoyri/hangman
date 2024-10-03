namespace HangmanApi.Models
{
    public class LeaderboardUserModel
    {
        public string UserName { get; set; }
        public int TotalScore { get; set; }
        public string ProfilePicture { get; set; } // Retourné sous forme de chaîne Base64
    }
}