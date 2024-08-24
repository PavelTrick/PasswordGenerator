using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PasswordGenerator.Server.BLL.Services;
using PasswordGenerator.Server.DAL;
using PasswordGenerator.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using PasswordGenerator.Server.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace PasswordGenerator.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 ILogger<AccountController> logger,
                                  IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("register"), AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state" });
            }

            var user = new ApplicationUser { UserName = model.Login, Email = model.Login };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                LoginResponse LoginResponse = new LoginResponse()
                {
                    AccessToken = Guid.NewGuid().ToString(),
                };
                return Ok(LoginResponse);
            }
            return BadRequest();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state" });
            }

            var result = await _signInManager.PasswordSignInAsync(model.Login, model.Password, false, false);

            if (result.Succeeded)
            {
                var user = _userManager.Users.FirstOrDefault(i => i.UserName == model.Login);
                LoginResponse LoginResponse = new LoginResponse()
                {
                    AccessToken = Guid.NewGuid().ToString(),
                };

                return Ok(LoginResponse);
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}
