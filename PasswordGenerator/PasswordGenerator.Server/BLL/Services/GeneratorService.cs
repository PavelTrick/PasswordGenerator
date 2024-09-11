using NuGet.Packaging;
using PasswordGenerator.Server.DAL;
using PasswordGenerator.Server.DAL.Models;
using PasswordGenerator.Server.Models;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

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

        public List<Password> GeneratePasswords(PasswordRequest passwordRequest, GeneratedPasswords generatedPasswordsResult, string userId)
        {
            List<Password> result = new List<Password>();
            int amount = passwordRequest.Amount;

            Stopwatch generateTime = new Stopwatch();
            Stopwatch verifyTime = new Stopwatch();

            if (passwordRequest.UseSimpleGenerator)
            {
                return SimpleGenerate(passwordRequest, result, generatedPasswordsResult, amount, userId, generateTime, verifyTime);
            }

            return GenerateViaHash(passwordRequest, result, generatedPasswordsResult, amount, userId, generateTime, verifyTime);
        }

        private List<Password> GenerateViaHash(PasswordRequest passwordRequest, List<Password> result, GeneratedPasswords generatedPasswordsResult, int amount, string userId, Stopwatch generateTime, Stopwatch verifyTime)
        {
            string characterSet = GetAlphabet(passwordRequest);
            int lastCount = _context.Passwords
                .Where(password => password.UserIdentifier == userId)
                .AsEnumerable()
                .Select(userPassword => (int?)userPassword.CodeHashCounter)
                .DefaultIfEmpty(0)
                .Max() ?? 0;

            lastCount++;
            generateTime.Start();

            while (amount > 0)
            {
                if (generatedPasswordsResult.Statistics.Count > MAX_ITERATION_COUNT)
                {
                    throw new Exception($"The number of iterations has reached its maximum {MAX_ITERATION_COUNT}");
                }

                generateTime.Start();
                List<Password> newPasswords = GenerateHashPasswords(passwordRequest, characterSet, amount, ref lastCount, result);
                generateTime.Stop();

                verifyTime.Start();

                var existingPasswords = _context.Passwords
                 .Where(p => p.UserIdentifier == userId && newPasswords.Select(i => i.Code).Contains(p.Code))
                 .Select(p => p.Code)
                 .ToList();
                lastCount += existingPasswords.Count;

                verifyTime.Stop();

                var unique = newPasswords.Where(newPassword => !existingPasswords.Contains(newPassword.Code)).ToList();
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

        private List<Password> GenerateHashPasswords(PasswordRequest passwordRequest, string characterSet, int amount, ref int lastCount, List<Password> exceptList)
        {
            HashSet<string> passwords = new HashSet<string>(exceptList.Select(i => i.Code));
            List<Password> result = new List<Password>();
            Random _random = new Random();
            var passwordBuilder = new System.Text.StringBuilder(passwordRequest.Length);
            int totalRequired = amount + exceptList.Count;
            int passwordHashSetCount = passwords.Count;

            while (passwords.Count < totalRequired)
            {
                var newPassword = GenerateUniquePassword(passwordRequest.Length, lastCount, characterSet);
                passwords.Add(newPassword);

                if (passwords.Count > passwordHashSetCount)
                {
                    passwordHashSetCount = passwords.Count;
                    result.Add(new Password
                    {
                        Code = newPassword,
                        CodeHashCounter = lastCount
                    });
                }

                lastCount++;
            }

            return result.ToList();
        }

        public static string GenerateUniquePassword(int length, int counter, string characterSet)
        {
            string hash = ComputeHash(counter.ToString());
            string password = ConvertToBase62(hash, characterSet);

            while (password.Length < length)
            {
                hash = ComputeHash(password);
                password += ConvertToBase62(hash, characterSet);
            }

            return password.Substring(0, length);
        }

        private static string ComputeHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder();

                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        private static string ConvertToBase62(string hash, string characterSet)
        {
            var base62Builder = new StringBuilder();
            foreach (char c in hash)
            {
                int index = Math.Abs(c % characterSet.Length);
                base62Builder.Append(characterSet[index]);
            }
            return base62Builder.ToString();
        }

        private List<Password> SimpleGenerate(PasswordRequest passwordRequest, List<Password> result, GeneratedPasswords generatedPasswordsResult, int amount, string userId, Stopwatch generateTime, Stopwatch verifyTime)
        {
            string characterSet = GetAlphabet(passwordRequest);

            while (amount > 0)
            {
                if (generatedPasswordsResult.Statistics.Count > MAX_ITERATION_COUNT)
                {
                    throw new Exception($"The number of iterations has reached its maximum {MAX_ITERATION_COUNT}");
                }

                generateTime.Start();
                List<string> newPasswords = Generate(passwordRequest, characterSet, amount, result.Select(p => p.Code).ToHashSet());
                generateTime.Stop();

                verifyTime.Start();

                var existingPasswords = _context.Passwords
                 .Where(p => p.UserIdentifier == userId && newPasswords.Contains(p.Code))
                 .Select(p => p.Code)
                 .ToList();

                verifyTime.Stop();

                var unique = newPasswords.Except(existingPasswords).ToList();

                unique.ForEach(uniqueCode =>
                {
                    result.Add(new Password
                    {
                        Code = uniqueCode,
                    });
                });

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
