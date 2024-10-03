using HangmanApi.Data;
using HangmanApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HangmanApi.Services
{
    public class ScoreService : IScoreService
    {
        private readonly HangmanDbContext _context;
        private double _comboMultiplier;
        private double _points;

        public ScoreService(HangmanDbContext context)
        {
            _context = context;
            _comboMultiplier = 1.0;
            _points = 0;
        }

        public async Task UpdateScoreAsync(int userId, int score)
        {
            var userScore = await _context.Scores.SingleOrDefaultAsync(s => s.UserId == userId);
            if (userScore != null)
            {
                userScore.TotalScore = score;
                userScore.LastUpdated = DateTime.UtcNow;
                _context.Scores.Update(userScore);
            }
            else
            {
                userScore = new Score
                {
                    UserId = userId,
                    TotalScore = score,
                    LastUpdated = DateTime.UtcNow
                };
                _context.Scores.Add(userScore);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Score> GetScoreAsync(int userId)
        {
            return await _context.Scores.SingleOrDefaultAsync(s => s.UserId == userId);
        }

        public void ResetPoints()
        {
            _points = 0;
        }

        public void CalculatePoints(int wordLength, string difficulty, bool isBonus)
        {
            double basePoints = wordLength * 10; // Exemple de calcul de base
            double difficultyMultiplier = difficulty switch
            {
                "easy" => 1.0,
                "medium" => 1.5,
                "hard" => 2.0,
                _ => 1.0
            };

            double bonusPoints = isBonus ? 50 : 0; // Points bonus

            _points = (basePoints + bonusPoints) * _comboMultiplier * difficultyMultiplier;
        }

        public double ComboMultiplier
        {
            get => _comboMultiplier;
            set => _comboMultiplier = value;
        }

        public double Points => _points;
    }
}