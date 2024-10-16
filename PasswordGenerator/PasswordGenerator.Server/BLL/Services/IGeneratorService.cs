using PasswordGenerator.Server.Models;

namespace PasswordGenerator.Server.BLL.Services
{
    public interface IGeneratorService
    {
        Task GeneratePasswords(PasswordRequest passwordRequest, int generateAmount = 0);
    }
}
