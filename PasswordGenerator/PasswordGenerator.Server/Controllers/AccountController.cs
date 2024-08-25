using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PasswordGenerator.Server.Models;
using PasswordGenerator.Server.DAL.Models;
using Microsoft.AspNetCore.Authorization;

namespace PasswordGenerator.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
                LoginResponse LoginResponse = new LoginResponse()
                {
                    AccessToken = Guid.NewGuid().ToString(),
                };

                return Ok(LoginResponse);
            }
            else
            {
                return Unauthorized();
            }
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
