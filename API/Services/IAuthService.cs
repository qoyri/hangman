using HangmanApi.Models;

namespace HangmanApi.Services
{
    public interface IAuthService
    {
        User Register(string username, string password);
        bool VerifyPassword(string enteredPassword, string storedHashedPassword);
        User GetByUserName(string username);
        string GenerateJwtToken(User user); // Assurez-vous que cette méthode est là
    }
}