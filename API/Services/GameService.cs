using HangmanApi.Data;
using HangmanApi.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace HangmanApi.Services
{
    public class GameService : IGameService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<GameService> _logger;
        private readonly IScoreService _scoreService;

        public GameService(IServiceScopeFactory scopeFactory, ILogger<GameService> logger, IScoreService scoreService)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _scoreService = scoreService;
        }

        private int GetUserIdFromClaimsPrincipal(ClaimsPrincipal user)
        {
            if (user == null)
                throw new Exception("User claims principal is null");

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new Exception("User ID claim not found");

            return int.Parse(userIdClaim.Value);
        }

        public async Task<string> GenerateNewWordAsync(ClaimsPrincipal user, string difficulty, bool resetCombo)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HangmanDbContext>();
            var userId = GetUserIdFromClaimsPrincipal(user);

            var userExists = await context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                throw new Exception($"User with Id {userId} does not exist.");

            var validDifficulties = new[] { "easy", "medium", "hard" };
            if (!validDifficulties.Contains(difficulty.ToLower()))
            {
                throw new ArgumentException("Invalid difficulty level. Valid values are: easy, medium, or hard.");
            }

            var existingGameState = await context.GameStates
                .Include(gs => gs.Word)
                .Where(gs => gs.UserId == userId && gs.IsActive)
                .OrderByDescending(gs => gs.Id)
                .FirstOrDefaultAsync();

            var random = new Random();
            var words = await context.Words.ToArrayAsync();
            var newWord = words[random.Next(words.Length)];

            double difficultyMultiplier = difficulty switch
            {
                "easy" => 1.0,
                "medium" => 1.5,
                "hard" => 2.0,
                _ => 1.0
            };

            if (existingGameState != null)
            {
                existingGameState.WordId = newWord.Id;
                existingGameState.Word = newWord;
                existingGameState.Difficulty = difficulty;
                if (resetCombo)
                {
                    existingGameState.ComboMultiplier = 1.0; // Réinitialiser le combo multiplier pour le nouveau mot
                }
                existingGameState.Score = 0; // Réinitialiser le score pour le nouveau mot
                context.GameStates.Update(existingGameState);
            }
            else
            {
                _scoreService.ResetPoints();
                _scoreService.ComboMultiplier = difficultyMultiplier;

                var gameState = new GameState
                {
                    UserId = userId,
                    WordId = newWord.Id,
                    Word = newWord,
                    Difficulty = difficulty,
                    ComboMultiplier = _scoreService.ComboMultiplier,
                    Score = 0, // Initialiser le score pour le premier mot
                    IsActive = true
                };

                context.GameStates.Add(gameState);
            }

            await context.SaveChangesAsync();

            _logger.LogInformation(
                $"New word generated: {newWord.Mot} with difficulty {difficulty} for user {userId} and combo multiplier {(resetCombo ? 1.0 : existingGameState.ComboMultiplier)}");
            
            // Ajoutons un log pour s'assurer que `newWord.Mot` contient la valeur correcte
            _logger.LogInformation($"Returning new word: {newWord.Mot}");
            
            return newWord.Mot;
        }

        public async Task IncreaseComboAsync(ClaimsPrincipal user)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HangmanDbContext>();
            var userId = GetUserIdFromClaimsPrincipal(user);

            var currentGameState = await context.GameStates
                .Where(gs => gs.UserId == userId && gs.IsActive)
                .OrderByDescending(gs => gs.Id)
                .LastOrDefaultAsync();

            if (currentGameState == null)
                throw new Exception("No active game state found for user");

            _scoreService.ComboMultiplier += 0.1;
            currentGameState.ComboMultiplier = _scoreService.ComboMultiplier;

            _logger.LogInformation(
                $"Before Update DB: GameStateID = {currentGameState.Id}, ComboMultiplier = {currentGameState.ComboMultiplier}");

            context.GameStates.Update(currentGameState);
            await context.SaveChangesAsync();

            _logger.LogInformation($"Combo multiplier increased to: {_scoreService.ComboMultiplier} for user {userId}");
        }

        public async Task ResetComboAsync(ClaimsPrincipal user)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HangmanDbContext>();
            var userId = GetUserIdFromClaimsPrincipal(user);

            var currentGameState = await context.GameStates
                .Where(gs => gs.UserId == userId && gs.IsActive)
                .OrderByDescending(gs => gs.Id)
                .LastOrDefaultAsync();

            if (currentGameState == null)
                throw new Exception("No active game state found for user");

            _scoreService.ComboMultiplier = 1.0;
            currentGameState.ComboMultiplier = _scoreService.ComboMultiplier;

            context.GameStates.Update(currentGameState);
            await context.SaveChangesAsync();

            _logger.LogInformation("Combo multiplier reset to 1.0");
        }

        public double GetComboMultiplier()
        {
            return _scoreService.ComboMultiplier;
        }

        public async Task<(double points, double comboMultiplier, string nextWord)> SuccessfulActionAsync(ClaimsPrincipal user, string guessedWord)
        {
            double points = 0;
            double comboMultiplier = 0;
            string nextWord = null;

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HangmanDbContext>();
            var userId = GetUserIdFromClaimsPrincipal(user);

            var currentGameState = await context.GameStates
                .Include(gs => gs.Word)
                .Where(gs => gs.UserId == userId && gs.IsActive)
                .OrderByDescending(gs => gs.Id)
                .LastOrDefaultAsync();

            if (currentGameState == null)
                throw new Exception("No active game state found for user");

            if (currentGameState.Word.Mot.Equals(guessedWord, StringComparison.OrdinalIgnoreCase))
            {
                comboMultiplier = Math.Round(currentGameState.ComboMultiplier, 2);
                _scoreService.CalculatePoints(currentGameState.Word.Mot.Length, currentGameState.Difficulty, false);
                points = Math.Round(_scoreService.Points * comboMultiplier, 2);
                
                currentGameState.Score = (int)points;
                comboMultiplier += 0.1;
                currentGameState.ComboMultiplier = comboMultiplier;
                
                var currentDifficulty = currentGameState.Difficulty;
                context.GameStates.Update(currentGameState);

                var userScore = await context.Scores.FirstOrDefaultAsync(s => s.UserId == userId);

                if (userScore == null)
                {
                    userScore = new Score
                    {
                        UserId = userId,
                        TotalScore = (int)points,
                        LastUpdated = DateTime.UtcNow
                    };
                    context.Scores.Add(userScore);
                }
                else
                {
                    userScore.TotalScore += (int)points;
                    userScore.LastUpdated = DateTime.UtcNow;
                    context.Scores.Update(userScore);
                }

                await context.SaveChangesAsync();

                nextWord = await GenerateNewWordAsync(user, currentDifficulty, false);

                _logger.LogInformation($"Score after combo multiplier applied: {points} for user {userId} with ComboMultiplier: {currentGameState.ComboMultiplier}");
                
                // Ajoutons un log pour confirmer le nouveau mot généré
                _logger.LogInformation($"Generated new word in SuccessfulActionAsync: {nextWord}");
                
                return (points, Math.Round(comboMultiplier, 2), nextWord);
            }
            else
            {
                _logger.LogWarning($"Incorrect word guessed: {guessedWord} by user {userId}");
                throw new Exception("Incorrect word guessed");
            }
        }

        public async Task<(int wordId, double newComboMultiplier)> GetHintAsync(ClaimsPrincipal user)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HangmanDbContext>();
            var userId = GetUserIdFromClaimsPrincipal(user);

            var currentGameState = await context.GameStates
                .Where(gs => gs.UserId == userId && gs.IsActive)
                .OrderByDescending(gs => gs.Id)
                .FirstOrDefaultAsync();

            if (currentGameState == null)
                throw new Exception("No active game state found for user");

            double newComboMultiplier = currentGameState.ComboMultiplier - 0.5;

            if (newComboMultiplier < 0)
            {
                throw new InvalidOperationException("Insufficient combo multiplier.");
            }

            currentGameState.ComboMultiplier = newComboMultiplier;
            context.GameStates.Update(currentGameState);
            await context.SaveChangesAsync();

            return (currentGameState.WordId, newComboMultiplier);
        }
    }
}