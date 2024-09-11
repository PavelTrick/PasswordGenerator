using PasswordGenerator.Server.DAL.Models;
using PasswordGenerator.Server.Models;

namespace PasswordGenerator.Server.BLL.Services
{
    public interface IGeneratorService
    {
        List<Password> GeneratePasswords(PasswordRequest passwordRequest, GeneratedPasswords generatedPasswordsResult, string userId);
    }
}
