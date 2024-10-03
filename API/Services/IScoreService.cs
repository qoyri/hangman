using HangmanApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HangmanApi.Services
{
    public interface IScoreService
    {
        void ResetPoints();
        void CalculatePoints(int wordLength, string difficulty, bool isBonus);
        double ComboMultiplier { get; set; }
        double Points { get; }
        Task UpdateScoreAsync(int userId, int score);
        Task<Score> GetScoreAsync(int userId);
    }
}