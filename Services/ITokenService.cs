using CommerceAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace CommerceAPI.Services
{
    public interface ITokenService
    {
        Task<TokenResult> CreateTokenAsync(User user, UserManager<User> userManager);
    }
}
