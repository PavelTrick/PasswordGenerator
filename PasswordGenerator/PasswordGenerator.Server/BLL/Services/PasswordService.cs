using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Server.DAL;
using PasswordGenerator.Server.DAL.Models;
using PasswordGenerator.Server.Models;
using System.Diagnostics;

namespace PasswordGenerator.Server.BLL.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly AppDbContext _context;
        private readonly IGeneratorService _generatorService;

        public PasswordService(AppDbContext context)
        {
            _context = context;
            _generatorService = new GeneratorService(_context);
        }

        public async Task<GeneratedPasswords> GenerateAndSave(PasswordRequest passwordRequest, string userId)
        {
            GeneratedPasswords result = new GeneratedPasswords();
            Stopwatch generateTime = new Stopwatch();
            Stopwatch totalTime = new Stopwatch();

            totalTime.Start();
            generateTime.Start();
            List<Password> generatedPassword = _generatorService.GeneratePasswords(passwordRequest, result, userId);
            generateTime.Stop();

            var dateNow = DateTime.UtcNow;
            List<Password> newPasswords = new List<Password>();

            foreach (var password in generatedPassword)
            {
                newPasswords.Add(new Password
                {
                    Code = password.Code,
                    CreatedAt = dateNow,
                    UserIdentifier = userId,
                    CodeHashCounter = password.CodeHashCounter
                });
            }

            _context.Passwords.AddRange(newPasswords);
            await _context.SaveChangesAsync();

            totalTime.Stop();

            result.GenerateTime = generateTime.ElapsedMilliseconds;
            result.ExecutionTime = totalTime.ElapsedMilliseconds;
            result.Passwords = generatedPassword.Select(i => i.Code).ToArray();

            return result;
        }

        public List<Password> GetAll()
        {
            return _context.Passwords.OrderByDescending(password => password.CreatedAt).ToList();
        }

        public List<Password> GetUserPasswords(string userId)
        {
            return _context.Passwords
                .Where(password => password.UserIdentifier == userId)
                ?.OrderByDescending(password => password.CreatedAt)
                ?.Take(1000)
                ?.ToList() ?? new List<Password>();
        }

        public Statistic GetUserPasswordStatistic(string userId)
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
            var result = await _context.Passwords
                               .Where(password => password.UserIdentifier == userId)
                               .ExecuteDeleteAsync();

            return result > 0;
        }
    }
}
