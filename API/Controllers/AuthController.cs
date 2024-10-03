using HangmanApi.Models;
using HangmanApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HangmanApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            var user = _authService.GetByUserName(model.Username);

            if (user == null || !_authService.VerifyPassword(model.Password, user.Password))
            {
                return Unauthorized("Invalid username or password");
            }

            var tokenString = _authService.GenerateJwtToken(user);
            return Ok(new { Token = tokenString });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest("Passwords do not match");
            }

            try
            {
                var user = _authService.Register(model.Username, model.Password);
                return Ok("User registered successfully");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}