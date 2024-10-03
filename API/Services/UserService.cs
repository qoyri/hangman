using HangmanApi.Data;
using HangmanApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HangmanApi.Services
{
    public class UserService : IUserService
    {
        private readonly HangmanDbContext _context;

        public UserService(HangmanDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<UserInfoModel> GetUserInfoAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return null;
            }

            var profilePicture = await _context.ProfilePictures
                .Where(p => p.UserId == userId)
                .Select(p => p.Picture)
                .FirstOrDefaultAsync();

            var totalScore = await _context.Scores
                .Where(s => s.UserId == userId)
                .Select(s => s.TotalScore)
                .FirstOrDefaultAsync();

            var gameState = await _context.GameStates
                .Include(gs => gs.Word)
                .Where(gs => gs.UserId == userId && gs.IsActive)
                .OrderByDescending(gs => gs.Id)
                .FirstOrDefaultAsync();

            return new UserInfoModel
            {
                UserName = user.UserName,
                TotalScore = totalScore,
                CurrentWord = gameState?.Word.Mot ?? string.Empty,
                CurrentWordScore = gameState?.Score ?? 0,
                ComboMultiplier = gameState?.ComboMultiplier ?? 1.0,
                Difficulty = gameState?.Difficulty, // Ajout de la difficulté
                ProfilePicture = profilePicture // Ajout de l'image de profil
            };
        }

        public async Task UpdateProfilePictureAsync(ClaimsPrincipal user, byte[] profilePicture)
        {
            var userId = GetUserIdFromClaimsPrincipal(user);
            var existingProfilePicture = await _context.ProfilePictures
                .SingleOrDefaultAsync(pp => pp.UserId == userId);

            if (existingProfilePicture != null)
            {
                existingProfilePicture.Picture = profilePicture;
                existingProfilePicture.CreatedAt = DateTime.UtcNow; // Assurer que `CreatedAt` est en UTC
                _context.ProfilePictures.Update(existingProfilePicture);
            }
            else
            {
                var newProfilePicture = new ProfilePicture
                {
                    UserId = userId,
                    Picture = profilePicture,
                    CreatedAt = DateTime.UtcNow // Assurer que `CreatedAt` est en UTC
                };
                _context.ProfilePictures.Add(newProfilePicture);
            }

            await _context.SaveChangesAsync();
        }

        private int GetUserIdFromClaimsPrincipal(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : throw new Exception("User ID claim not found");
        }
    }
}