using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuGet.ContentModel;
using NuGet.Packaging;
using PasswordGenerator.Server.DAL;
using PasswordGenerator.Server.DAL.Models;
using PasswordGenerator.Server.Models;
using System.Collections.Generic;
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
                List<string> generatedPassword = GeneratePasswords(passwordRequest, result, userId);
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

        private List<string> GeneratePasswords(PasswordRequest passwordRequest, GeneratedPasswords generatedPasswordsResult, string userId)
        {
            string characterSet = GetAlphabet(passwordRequest);
            HashSet<string> result = new HashSet<string>();
            int amount = passwordRequest.Amount;

            Stopwatch generateTime = new Stopwatch();
            Stopwatch verifyTime = new Stopwatch();

            while (amount > 0)
            {
                generateTime.Start();
                List<string> newPasswords = Generate(passwordRequest, characterSet, amount, result);
                generateTime.Stop();

                verifyTime.Start();

                //_context.Passwords
                //    .Where(p => p.UserIdentifier == passwordRequest.UserId && newPasswords.Contains(p.Code))
                //    .Select(p => p.Code)
                //    .ToList();

                var existingPasswords = _context.Passwords
                 .Where(p => p.UserIdentifier == userId && newPasswords.Contains(p.Code))
                 .Select(p => p.Code)
                 .ToList();

                //var unique = newPasswords.Where(newPassword => _context.Passwords.All(dbPassword =>
                //    dbPassword.UserIdentifier == passwordRequest.UserId &&
                //    dbPassword.Code != newPassword)).ToList();
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

            return passwords.Except(exceptList).Take(amount).ToList();
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
