using HangmanApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HangmanApi.Data;
using HangmanApi.Models;

namespace HangmanApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // sécuriser ce contrôleur
    public class ScoreController : ControllerBase
    {
        private readonly HangmanDbContext _context;

        public ScoreController(HangmanDbContext context)
        {
            _context = context;
        }

        [HttpGet("leaderboard")]
        public async Task<IActionResult> GetLeaderboardAsync()
        {
            try
            {
                var leaderboard = await _context.Scores
                    .Include(s => s.User) // Inclure les informations de l'utilisateur
                    .OrderByDescending(s => s.TotalScore)
                    .Select(s => new LeaderboardUserModel
                    {
                        UserName = s.User.UserName,
                        TotalScore = s.TotalScore,
                        ProfilePicture = _context.ProfilePictures
                                             .Where(p => p.UserId == s.UserId)
                                             .Select(p => Convert.ToBase64String(p.Picture))
                                             .FirstOrDefault() ??
                                         string.Empty // Renvoie une chaîne vide si l'image de profil n'est pas trouvée
                    })
                    .ToListAsync();

                return Ok(leaderboard);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the leaderboard.", details = ex.Message });
            }
        }
    }
}