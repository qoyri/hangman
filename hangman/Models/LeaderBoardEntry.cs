using System.Text.Json.Serialization;

public class LeaderboardEntry
{
    [JsonPropertyName("userName")] public string UserName { get; set; }

    [JsonPropertyName("totalScore")] public int TotalScore { get; set; }

    [JsonPropertyName("profilePicture")] public string ProfilePicture { get; set; }
}