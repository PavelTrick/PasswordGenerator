using PasswordGenerator.Server.Models;

namespace PasswordGenerator.Server.BLL.Services
{
    public interface IGeneratorService
    {
        List<string> GeneratePasswords(PasswordRequest passwordRequest, GeneratedPasswords generatedPasswordsResult, string userId);
    }
}
