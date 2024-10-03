using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HangmanApi.Data;
using HangmanApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace HangmanApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly HangmanDbContext _context;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _config;

        public AuthService(HangmanDbContext context, ILogger<AuthService> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        public User Register(string username, string password)
        {
            _logger.LogInformation("Registering a new user with username {UserName}", username);

            if (_context.Users.Any(u => u.UserName == username))
            {
                _logger.LogWarning("User with username {UserName} already exists", username);
                throw new InvalidOperationException("Username already exists.");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                UserName = username,
                Password = hashedPassword
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            _logger.LogInformation("User registered successfully: {UserName}", username);
            return user;
        }

        public bool VerifyPassword(string enteredPassword, string storedHashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHashedPassword);
        }

        public User GetByUserName(string username)
        {
            return _context.Users.FirstOrDefault(u => u.UserName == username);
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                    // Supprimez la ligne du rôle si non nécessaire
                }),
                Expires = DateTime.UtcNow.AddDays(90),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString; // Retournez juste la chaîne du token
        }
    }
}