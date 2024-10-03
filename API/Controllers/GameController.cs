using HangmanApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HangmanApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost("new-word")]
        [Authorize]
        public async Task<IActionResult> GenerateNewWord([FromBody] NewWordRequest request)
        {
            try
            {
                var user = User;
                var newWord = await _gameService.GenerateNewWordAsync(user, request.Difficulty, true);
                return Ok(new { word = newWord });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        public class NewWordRequest
        {
            public string Difficulty { get; set; }
        }

       [HttpPost("successful-action")]
        public async Task<IActionResult> SuccessfulAction([FromBody] string guessedWord)
        {
            try
            {
                var (points, comboMultiplier, nextWord) = await _gameService.SuccessfulActionAsync(User, guessedWord);
                return Ok(new { points, comboMultiplier, nextWord }); // Inclure nextWord dans la réponse
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("reset-combo")]
        public async Task<IActionResult> ResetCombo()
        {
            try
            {
                await _gameService.ResetComboAsync(User);
                return Ok(new
                    { Message = "Combo multiplier reset", ComboMultiplier = _gameService.GetComboMultiplier() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("hint")]
        public async Task<IActionResult> GetHint()
        {
            try
            {
                var (wordId, newComboMultiplier) = await _gameService.GetHintAsync(User);
                return Ok(new { wordId, newComboMultiplier });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
    }
}