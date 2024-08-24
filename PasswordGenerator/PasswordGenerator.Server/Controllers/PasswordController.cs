using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PasswordGenerator.Server.DAL.Models;
using PasswordGenerator.Server.DAL;
using PasswordGenerator.Server.BLL.Services;
using PasswordGenerator.Server.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace PasswordGenerator.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly IPasswordGeneratorService _passwordGeneratorService;

        public PasswordController(AppDbContext context)
        {
            _passwordGeneratorService = new PasswordGeneratorService(context);

        }

        [HttpGet("list")]
        public IActionResult GetUserPasswords()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return BadRequest("User is not authenticated");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var generatedPassword = _passwordGeneratorService.GetUserPasswords(userId);

            return Ok(generatedPassword);
        }

        [HttpGet("statistic")]
        public async Task<IActionResult> GetUserPasswordStatistic()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return BadRequest("User is not authenticated");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var generatedPassword = await _passwordGeneratorService.GetUserPasswordStatistic(userId);

            return Ok(generatedPassword);
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePasswords([FromBody] PasswordRequest request)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return BadRequest("User is not authenticated");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            GeneratedPasswords generatedPassword = await _passwordGeneratorService.GenerateAndSave(request, userId);

            return Ok(generatedPassword);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUserPassword()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return BadRequest("User is not authenticated");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool result = await _passwordGeneratorService.ClearUserPasswords(userId);

            return Ok(result);
        }
    }
}
