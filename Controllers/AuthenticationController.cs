using CommerceAPI.Models;
using CommerceAPI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;

        public AuthenticationController(UserManager<User> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] Register request)
        {
            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = false, Message = "Email exists!" });
            }

            var user = new User
            {
                Email = request.Email,
                UserName = request.Name
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = false, Message = "Failed to add user to database!" });
            }

            var token = await _tokenService.CreateTokenAsync(user, _userManager);

            return Ok(new Response { Status = true, Data = new { User = user, Token = token } });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] Login request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, request.Password))
            {
                var token = await _tokenService.CreateTokenAsync(user, _userManager);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Set to true if using HTTPS
                    SameSite = SameSiteMode.None,
                };

                Response.Cookies.Append("jwt", token.Result!, cookieOptions);

                return Ok(new Response { Status = true, Data = new { User = user, Token = token } });
            }



            return Unauthorized(new Response { Status = true, Message = "Invalid credentials" });
        }

        [HttpGet]
        [Route("check")]
        [Authorize]
        public IActionResult Check()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized(new Response { Status = false, Message = "Not authorized!" });
            }

            return Ok(new Response { Status = true, Message = "Authorized!" });
        }

        [HttpPost]
        [Route("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return Ok(new Response { Status = true, Message = "Logout successful!" });
        }
    }
}
