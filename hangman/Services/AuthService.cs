using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HangmanApp.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://hangman.api.qoyri.fr/") };
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            try
            {
                var loginRequest = new
                {
                    Username = username,
                    Password = password
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(loginRequest),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("api/Auth/login", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Login failed with status code {response.StatusCode}: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent);

                return loginResponse.Token;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during login: {ex.Message}");
                throw new Exception("Login failed. Please check your credentials and try again.", ex);
            }
        }

        public async Task<bool> RegisterAsync(string username, string password, string confirmPassword)
        {
            try
            {
                var registerRequest = new
                {
                    Username = username,
                    Password = password,
                    ConfirmPassword = confirmPassword
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(registerRequest),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("api/Auth/register", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Registration failed with status code {response.StatusCode}: {errorContent}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during registration: {ex.Message}");
                throw new Exception("Registration failed. Please check your details and try again.", ex);
            }
        }

        public async Task<UserInfo> GetUserInfoAsync(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/Protected/userinfo");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception(
                        $"Failed to retrieve user info with status code {response.StatusCode}: {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<UserInfo>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during user info retrieval: {ex.Message}");
                throw new Exception("Failed to retrieve user info. Please try again.", ex);
            }
        }

        public async Task<bool> UploadProfilePicture(byte[] fileBytes, string fileName, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var content = new MultipartFormDataContent();
                var byteArrayContent = new ByteArrayContent(fileBytes);
                byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg"); // Or "image/png" based on file type
                content.Add(byteArrayContent, "file", fileName);

                var response = await _httpClient.PostAsync("api/Profile/ProfilePicture", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Upload failed: {response.StatusCode} - {errorResponse}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading profile picture: {ex.Message}");
                return false;
            }
        }
    }
}