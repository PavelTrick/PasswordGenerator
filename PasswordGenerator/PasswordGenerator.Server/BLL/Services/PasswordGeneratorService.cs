using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Server.DAL;
using PasswordGenerator.Server.DAL.Models;
using PasswordGenerator.Server.Models;
using System.Diagnostics;

namespace PasswordGenerator.Server.BLL.Services
{
    public class PasswordGeneratorService : IPasswordGeneratorService
    {
        const string SpecialChars = "!@#$%^&*()";
        const string Numbers = "0123456789";
        const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string Lowercase = "abcdefghijklmnopqrstuvwxyz";

        private static readonly Random _random = new Random();
        private readonly AppDbContext _context;

        public PasswordGeneratorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<GeneratedPasswords> GenerateAndSave(PasswordRequest passwordRequest, string userId)
        {
            try
            {
                GeneratedPasswords result = new GeneratedPasswords();

                Stopwatch generateTime = new Stopwatch();
                Stopwatch totalTime = new Stopwatch();
                totalTime.Start();
                generateTime.Start();
                List<string> generatedPassword = GeneratePasswords(passwordRequest, result);
                generateTime.Stop();

                foreach (var password in generatedPassword)
                {
                    var dateNow = DateTime.UtcNow;

                    var passwordModel = new Password
                    {
                        Code = password,
                        CreatedAt = dateNow,
                        UserIdentifier = userId,

                    };

                    _context.Passwords.Add(passwordModel);
                }

                await _context.SaveChangesAsync();

                totalTime.Stop();

                result.GenerateTime = generateTime.ElapsedMilliseconds;
                result.ExecutionTime = totalTime.ElapsedMilliseconds;
                result.Passwords = generatedPassword.ToArray();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Password> GetAll()
        {
            return _context.Passwords.OrderByDescending(password => password.CreatedAt).ToList();
        }

        public List<Password> Get()
        {
            return _context.Passwords
                .OrderByDescending(password => password.CreatedAt).ToList();
        }

        public List<Password> GetUserPasswords(string userId)
        {
            return _context.Passwords
                .Where(password => password.UserIdentifier == userId)
                ?.OrderByDescending(password => password.CreatedAt)
                ?.Take(1000)
                ?.ToList() ?? new List<Password>();
        }

        public async Task<Statistic> GetUserPasswordStatistic(string userId)
        {
            Statistic statistic = new Statistic();
            var passwords = _context.Passwords.Where(password => password.UserIdentifier == userId);

            statistic.TotalCount = passwords.Count();
            statistic.DuplicateCount = passwords
                .GroupBy(password => password.Code)
                .Where(group => group.Count() > 1)
                .Count();

            return statistic;
        }

        public async Task<bool> ClearUserPasswords(string userId)
        {
            var passwords = _context.Passwords.Where(password => password.UserIdentifier == userId).ToArray();
            _context.Passwords.RemoveRange(passwords);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        private List<string> GeneratePasswords(PasswordRequest passwordRequest, GeneratedPasswords generatedPasswordsResult)
        {
            string characterSet = GetAlphabet(passwordRequest);
            List<string> result = new List<string>();
            int amount = passwordRequest.Amount;

            Stopwatch generateTime = new Stopwatch();
            Stopwatch verifyTime = new Stopwatch();

            while (amount != 0)
            {
                generateTime.Start();
                List<string> newPasswords = Generate(passwordRequest, characterSet, amount);
                generateTime.Stop();

                verifyTime.Start();
                var unique = newPasswords.Where(newPassword => _context.Passwords.All(dbPassword => dbPassword.Code != newPassword)).ToList();
                verifyTime.Stop();

                result.AddRange(unique);
                amount = newPasswords.Count - unique.Count;

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

            return result;
        }

        private List<string> Generate(PasswordRequest passwordRequest, string characterSet, int amount)
        {
            List<string> passwords = new List<string>();

            while (passwords.Count < amount)
            {
                var newPassword = new string(Enumerable.Repeat(characterSet, passwordRequest.Length).Select(s => s[_random.Next(s.Length)]).ToArray());

                if (!passwords.Contains(newPassword))
                {
                    passwords.Add(newPassword);
                }
            }

            return passwords;
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
