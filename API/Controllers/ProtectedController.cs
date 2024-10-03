using HangmanApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;

namespace HangmanApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProtectedController : ControllerBase
    {
        private readonly IUserService _userService;

        public ProtectedController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("userinfo")]
        [Authorize]
        public async Task<IActionResult> GetUserInfo()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("User not found");
            }

            var userInfo = await _userService.GetUserInfoAsync(int.Parse(userId));
            if (userInfo == null)
            {
                return NotFound("User not found");
            }

            return Ok(userInfo);
        }
    }
}