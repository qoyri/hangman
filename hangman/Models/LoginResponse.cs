using System.Text.Json.Serialization;

public class LoginResponse
{
    [JsonPropertyName("token")] public string Token { get; set; }
}