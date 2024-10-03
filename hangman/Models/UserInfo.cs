using System.Text.Json.Serialization;

public class UserInfo
{
    [JsonPropertyName("userName")] public string UserName { get; set; }
    [JsonPropertyName("totalScore")] public int TotalScore { get; set; }
    [JsonPropertyName("currentWord")] public string CurrentWord { get; set; }
    [JsonPropertyName("currentWordScore")] public int CurrentWordScore { get; set; }
    [JsonPropertyName("comboMultiplier")] public double ComboMultiplier { get; set; }
    [JsonPropertyName("difficulty")] public string Difficulty { get; set; }
    [JsonPropertyName("profilePicture")] public string ProfilePicture { get; set; }
}