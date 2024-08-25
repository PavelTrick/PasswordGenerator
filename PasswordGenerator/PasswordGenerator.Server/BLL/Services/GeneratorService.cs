using NuGet.Packaging;
using PasswordGenerator.Server.DAL;
using PasswordGenerator.Server.Models;
using System.Diagnostics;

namespace PasswordGenerator.Server.BLL.Services
{
    public class GeneratorService : IGeneratorService
    {
        const int MAX_ITERATION_COUNT = 200;
        const string SpecialChars = "!@#$%^&*()";
        const string Numbers = "0123456789";
        const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string Lowercase = "abcdefghijklmnopqrstuvwxyz";

        private readonly AppDbContext _context;

        public GeneratorService(AppDbContext context)
        {
            _context = context;
        }

        public List<string> GeneratePasswords(PasswordRequest passwordRequest, GeneratedPasswords generatedPasswordsResult, string userId)
        {
            string characterSet = GetAlphabet(passwordRequest);
            HashSet<string> result = new HashSet<string>();
            int amount = passwordRequest.Amount;

            Stopwatch generateTime = new Stopwatch();
            Stopwatch verifyTime = new Stopwatch();

            while (amount > 0)
            {
                if (generatedPasswordsResult.Statistics.Count > MAX_ITERATION_COUNT)
                {
                    throw new Exception($"The number of iterations has reached its maximum {MAX_ITERATION_COUNT}");
                }

                generateTime.Start();
                List<string> newPasswords = Generate(passwordRequest, characterSet, amount, result);
                generateTime.Stop();

                verifyTime.Start();

                var existingPasswords = _context.Passwords
                 .Where(p => p.UserIdentifier == userId && newPasswords.Contains(p.Code))
                 .Select(p => p.Code)
                 .ToList();

                verifyTime.Stop();

                var unique = newPasswords.Except(existingPasswords).ToList();

                result.AddRange(unique);
                amount = passwordRequest.Amount - result.Count;

                var duplicationCount = newPasswords.Except(unique).ToList().Count;

                generatedPasswordsResult.Statistics.Add(new GenerateStatistic()
                {
                    LogTime = DateTime.UtcNow,
                    DuplicationCount = duplicationCount,
                    GeneratePasswordTime = generateTime.ElapsedMilliseconds,
                    VerifyDBUniquesTime = verifyTime.ElapsedMilliseconds,
                });

                generateTime.Reset();
                verifyTime.Reset();
            }

            return result.ToList();
        }

        private List<string> Generate(PasswordRequest passwordRequest, string characterSet, int amount, HashSet<string> exceptList)
        {
            HashSet<string> passwords = new HashSet<string>(exceptList);
            Random _random = new Random();
            var passwordBuilder = new System.Text.StringBuilder(passwordRequest.Length);
            int totalRequired = amount + exceptList.Count;

            while (passwords.Count < totalRequired)
            {
                passwordBuilder.Clear();

                for (int i = 0; i < passwordRequest.Length; i++)
                {
                    passwordBuilder.Append(characterSet[_random.Next(characterSet.Length)]);
                }

                var newPassword = passwordBuilder.ToString();

                passwords.Add(newPassword);
            }

            return passwords.Except(exceptList).ToList();
        }

        private string GetAlphabet(PasswordRequest passwordRequest)
        {
            string characterSet = String.Empty;

            if (passwordRequest.IncludeSpecial)
            {
                characterSet += SpecialChars;
            }

            if (passwordRequest.IncludeNumbers)
            {
                characterSet += Numbers;
            }

            if (passwordRequest.IncludeUppercase)
            {
                characterSet += Uppercase;
            }

            if (passwordRequest.IncludeLowercase)
            {
                characterSet += Lowercase;
            }

            return characterSet;
        }
    }
}
