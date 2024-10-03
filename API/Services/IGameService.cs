using System.Security.Claims;
using System.Threading.Tasks;

namespace HangmanApi.Services
{
    public interface IGameService
    {
        Task<string> GenerateNewWordAsync(ClaimsPrincipal user, string difficulty, bool resetCombo);
        Task IncreaseComboAsync(ClaimsPrincipal user);
        Task ResetComboAsync(ClaimsPrincipal user);
        double GetComboMultiplier();
        Task<(double points, double comboMultiplier, string nextWord)> SuccessfulActionAsync(ClaimsPrincipal user, string guessedWord);
        Task<(int wordId, double newComboMultiplier)> GetHintAsync(ClaimsPrincipal user);

    }
}