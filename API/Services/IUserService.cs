using System.Security.Claims;
using System.Threading.Tasks;
using HangmanApi.Models;

namespace HangmanApi.Services
{
    public interface IUserService
    {
        Task<UserInfoModel> GetUserInfoAsync(int userId);
        Task UpdateProfilePictureAsync(ClaimsPrincipal user, byte[] profilePicture);
    }
}