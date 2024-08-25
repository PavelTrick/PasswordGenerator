using Microsoft.AspNetCore.Mvc;
using PasswordGenerator.Server.DAL;
using PasswordGenerator.Server.BLL.Services;
using PasswordGenerator.Server.Models;
using System.Security.Claims;

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
        public IActionResult GetUserPasswordStatistic()
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

            var generatedPassword = _passwordGeneratorService.GetUserPasswordStatistic(userId);

            return Ok(generatedPassword);
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePasswords([FromBody] PasswordRequest request)
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

                GeneratedPasswords generatedPassword = await _passwordGeneratorService.GenerateAndSave(request, userId);

                return Ok(generatedPassword);
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
    }
}
