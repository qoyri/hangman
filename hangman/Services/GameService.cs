using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;

namespace HangmanApp.Services
{
    public class GameService
    {
        private readonly HttpClient _httpClient;

        public GameService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://hangman.api.qoyri.fr/")
            };
        }

        public async Task<string> GetNewWordAsync(string difficulty, string token)
        {
            var content = new StringContent(JsonSerializer.Serialize(new { difficulty }), System.Text.Encoding.UTF8,
                "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync("/api/Game/new-word", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(result);
            return jsonDoc.RootElement.GetProperty("word").GetString();
        }

        public async Task<(int points, double comboMultiplier, string nextWord)> ConfirmWordAsync(string word,
            string token)
        {
            var content = new StringContent(JsonSerializer.Serialize(word), System.Text.Encoding.UTF8,
                "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync("/api/Game/successful-action", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(result);

            int points = jsonDoc.RootElement.GetProperty("points").GetInt32();
            double comboMultiplier = jsonDoc.RootElement.GetProperty("comboMultiplier").GetDouble();
            string nextWord = jsonDoc.RootElement.GetProperty("nextWord").GetString();

            return (points, comboMultiplier, nextWord);
        }

        // New hint method
        public async Task<(int wordId, double comboMultiplier)> GetHintAsync(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync("/api/Game/hint");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(result);

            int wordId = jsonDoc.RootElement.GetProperty("wordId").GetInt32();
            double comboMultiplier = jsonDoc.RootElement.GetProperty("newComboMultiplier").GetDouble();
            return (wordId, comboMultiplier);
        }
    }
}