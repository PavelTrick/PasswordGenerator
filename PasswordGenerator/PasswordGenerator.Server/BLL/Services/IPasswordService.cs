using PasswordGenerator.Server.DAL.Models;
using PasswordGenerator.Server.Models;

namespace PasswordGenerator.Server.BLL.Services
{
    public interface IPasswordService
    {
        List<Password> GetAll();
        Task<GeneratedPasswords> GenerateAndSave(PasswordRequest passwordRequest, string userId);
        List<Password> GetUserPasswords(string userId);
        Statistic GetUserPasswordStatistic(string userId);
        Task<bool> ClearUserPasswords(string userId);
    }
}
