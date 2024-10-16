using Microsoft.AspNetCore.Mvc;
using PasswordGenerator.Server.DAL;
using PasswordGenerator.Server.BLL.Services;
using PasswordGenerator.Server.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace PasswordGenerator.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly IPasswordService _passwordGeneratorService;

        public PasswordController(AppDbContext context)
        {
            _passwordGeneratorService = new PasswordService(context);

        }

        [HttpGet("list")]
        public IActionResult GetUserPasswords()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return BadRequest("User is not authenticated");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (String.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found");
            }

            var generatedPassword = _passwordGeneratorService.GetUserPasswords(userId);

            return Ok(generatedPassword);
        }

        [HttpGet("statistic")]
        public IActionResult GetPasswordStatistic()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return BadRequest("User is not authenticated");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (String.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found");
            }

            var generatedPassword = _passwordGeneratorService.GetPasswordsStatistic();

            return Ok(generatedPassword);
        }


        [HttpGet("generate/log")]
        public IActionResult GetGenerateLog()
        {
            var generatedPassword = _passwordGeneratorService.GetGenerateLogs();

            return Ok(generatedPassword);
        }

        [HttpPost("new")]
        public async Task<IActionResult> GetNewUserPasswords([FromBody] PasswordRequest request)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return BadRequest("User is not authenticated");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (String.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found");
            }

            try
            {
                List<string> generatedPassword = await _passwordGeneratorService.TakePasswords(request, userId);

                return Ok(generatedPassword);
            }
            catch (Exception ex)
            {
                return Problem($"{ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpPost("generate")]
        public async Task<IActionResult> GeneratePasswords([FromBody] PasswordRequest request)
        {
            try
            {
                await _passwordGeneratorService.GeneratePasswords(request);
                return Ok();
            }
            catch (Exception ex)
            {
                return Problem($"{ex.Message}");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUserPassword()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return BadRequest("User is not authenticated");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (String.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found");
            }

            bool result = await _passwordGeneratorService.ClearUserPasswords(userId);

            return Ok(result);
        }

        [HttpDelete("store")]
        public async Task<IActionResult> DeletePasswordStore()
        {
            bool result = await _passwordGeneratorService.ClearPasswordStore();

            return Ok(result);
        }
    }
}
