using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Server.DAL.Models;
using PasswordGenerator.Server.Models;

namespace PasswordGenerator.Server.BLL.Services
{
    public interface IPasswordGeneratorService
    {
        List<Password> GetAll();
        Task<GeneratedPasswords> GenerateAndSave(PasswordRequest passwordRequest, string userId);
        List<Password> GetUserPasswords(string userId);
        Task<Statistic> GetUserPasswordStatistic(string userId);
         Task<bool> ClearUserPasswords(string userId);
    }
}
