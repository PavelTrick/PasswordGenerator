using PasswordGenerator.Server.DAL.Models;
using PasswordGenerator.Server.Models;

namespace PasswordGenerator.Server.BLL.Services
{
    public interface IPasswordService
    {
        List<Password> GetAll();
        Task<List<string>> TakePasswords(PasswordRequest passwordRequest, string userId);
        List<Password> GetUserPasswords(string userId);
        Statistic GetPasswordsStatistic();
        List<GenerateStatistic> GetGenerateLogs();
        Task<bool> ClearUserPasswords(string userId);
        Task<bool> ClearPasswordStore();
        Task GeneratePasswords(PasswordRequest passwordRequest);
    }
}
