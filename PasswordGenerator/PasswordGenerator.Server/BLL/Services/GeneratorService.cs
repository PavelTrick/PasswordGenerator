using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using PasswordGenerator.Server.DAL;
using PasswordGenerator.Server.DAL.Models;
using PasswordGenerator.Server.Models;
using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace PasswordGenerator.Server.BLL.Services
{
    public class GeneratorService : IGeneratorService
    {
        const int MAX_ITERATION_COUNT = 100;
        const string SpecialChars = "!@#$%^&*()";
        const string Numbers = "0123456789";
        const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string Lowercase = "abcdefghijklmnopqrstuvwxyz";

        const int InStockRule = 10;

        private readonly AppDbContext _context;

        public GeneratorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task GeneratePasswords(PasswordRequest passwordRequest, int generateAmount = 0)
        {
            try
            {
                GeneratedPasswords generatedPasswordsResult = new GeneratedPasswords();
                Stopwatch totalTime = new Stopwatch();
                totalTime.Start();

                if (generateAmount == 0)
                {
                    var passwrodsCount = _context.Passwords.Count();
                    var maxUserPasswordCount = _context.Users
                        .Include(u => u.UserPasswords)
                        .AsEnumerable()
                        .Select(u => u.UserPasswords.Count)
                        .DefaultIfEmpty(0)
                        .Max();

                    var countDifference = passwrodsCount - maxUserPasswordCount;
                    bool needGenerate = countDifference < InStockRule;

                    if (!needGenerate)
                    {
                        return;
                    }

                    generateAmount = InStockRule - countDifference;
                }

                passwordRequest.Amount = generateAmount;
                int amount = generateAmount;

                if (passwordRequest.UseSimpleGenerator)
                {
                    SimpleGenerate(passwordRequest, generatedPasswordsResult, amount);
                }
                else
                {
                    GenerateViaHash(passwordRequest, generatedPasswordsResult, amount);
                }

                if (generatedPasswordsResult.Passwords.Any())
                {
                    _context.Passwords.AddRange(generatedPasswordsResult.Passwords);
                }

                totalTime.Stop();
                GenerateStatistic generateStatistic = new GenerateStatistic
                {
                    PasswordAmount = generateAmount,
                    StatisticIterations = generatedPasswordsResult.Statistics,
                    TotalTime = totalTime.ElapsedMilliseconds
                };

                _context.GenerateStatistics.Add(generateStatistic);

                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void GenerateViaHash(PasswordRequest passwordRequest, GeneratedPasswords generatedPasswordsResult, int amount)
        {
            List<Password> result = new List<Password>();
            string characterSet = GetAlphabet(passwordRequest);
            int lastCount = _context.Passwords
                .Select(userPassword => (int?)userPassword.CodeHashCounter)
                .DefaultIfEmpty(0)
                .Max() ?? 0;

            lastCount++;
            Stopwatch generateTime = new Stopwatch();
            Stopwatch verifyTime = new Stopwatch();

            int iterationCount = 1;

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
                 .Where(p => newPasswords.Select(i => i.Code).Contains(p.Code))
                 .Select(p => p.Code)
                 .ToList();
                lastCount += existingPasswords.Count;
                verifyTime.Stop();

                var unique = newPasswords.Where(newPassword => !existingPasswords.Contains(newPassword.Code)).ToList();
                result.AddRange(unique);

                amount = passwordRequest.Amount - result.Count;

                var duplicationCount = newPasswords.Except(unique).ToList().Count;

                generatedPasswordsResult.Statistics.Add(new GenerateStatisticIteration()
                {
                    IterationNumber = iterationCount,
                    LogTime = DateTime.UtcNow,
                    DuplicationCount = duplicationCount,
                    GeneratePasswordTime = generateTime.ElapsedMilliseconds,
                    VerifyDBUniquesTime = verifyTime.ElapsedMilliseconds,
                });

                iterationCount++;

                generateTime.Reset();
                verifyTime.Reset();
            }

            generatedPasswordsResult.Passwords = result;
        }

        private List<Password> GenerateHashPasswords(PasswordRequest passwordRequest, string characterSet, int amount, ref int lastCount, List<Password> exceptList)
        {
            HashSet<string> passwords = new HashSet<string>(exceptList.Select(i => i.Code));
            List<Password> result = new List<Password>();
            Random _random = new Random();
            var passwordBuilder = new System.Text.StringBuilder(passwordRequest.Length);
            int totalRequired = amount + exceptList.Count;
            int passwordHashSetCount = passwords.Count;
            int counter = 0;

            while (passwords.Count < totalRequired && counter < amount * 1000)
            {
                var newPassword = GenerateUniquePassword(passwordRequest.Length, lastCount, characterSet);
                passwords.Add(newPassword);

                if (passwords.Count > passwordHashSetCount)
                {
                    passwordHashSetCount = passwords.Count;
                    result.Add(new Password
                    {
                        Code = newPassword,
                        CodeHashCounter = lastCount,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                lastCount++;
                counter++;
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

        private void SimpleGenerate(PasswordRequest passwordRequest, GeneratedPasswords generatedPasswordsResult, int amount)
        {
            List<Password> result = new List<Password>();
            string characterSet = GetAlphabet(passwordRequest);
            int iterationCount = 1;

            Stopwatch generateTime = new Stopwatch();
            Stopwatch verifyTime = new Stopwatch();

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
                 .Where(p => newPasswords.Contains(p.Code))
                 .Select(p => p.Code)
                 .ToList();
                verifyTime.Stop();

                var unique = newPasswords.Except(existingPasswords).ToList();

                unique.ForEach(uniqueCode =>
                {
                    result.Add(new Password
                    {
                        Code = uniqueCode,
                        CreatedAt = DateTime.UtcNow
                    });
                });

                amount = passwordRequest.Amount - result.Count;

                var duplicationCount = newPasswords.Except(unique).ToList().Count;

                generatedPasswordsResult.Statistics.Add(new GenerateStatisticIteration()
                {
                    IterationNumber = iterationCount,
                    LogTime = DateTime.UtcNow,
                    DuplicationCount = duplicationCount,
                    GeneratePasswordTime = generateTime.ElapsedMilliseconds,
                    VerifyDBUniquesTime = verifyTime.ElapsedMilliseconds,
                });

                generateTime.Reset();
                verifyTime.Reset();
            }

            generatedPasswordsResult.Passwords = result;
        }

        private List<string> Generate(PasswordRequest passwordRequest, string characterSet, int amount, HashSet<string> exceptList)
        {
            HashSet<string> passwords = new HashSet<string>(exceptList);
            Random _random = new Random();
            var passwordBuilder = new System.Text.StringBuilder(passwordRequest.Length);
            int totalRequired = amount + exceptList.Count;
            int counter = 0;

            while (passwords.Count < totalRequired && counter < amount * 1000)
            {
                passwordBuilder.Clear();

                for (int i = 0; i < passwordRequest.Length; i++)
                {
                    passwordBuilder.Append(characterSet[_random.Next(characterSet.Length)]);
                }

                var newPassword = passwordBuilder.ToString();

                passwords.Add(newPassword);
                counter++;
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
