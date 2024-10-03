using HangmanApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace HangmanApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IUserService _userService;

        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("ProfilePicture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            if (file.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                try
                {
                    await _userService.UpdateProfilePictureAsync(User, fileBytes);
                    return Ok(new { message = "Profile picture uploaded successfully" });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { error = ex.Message });
                }
            }
            else
            {
                return BadRequest(new { error = "Invalid file" });
            }
        }
    }
}