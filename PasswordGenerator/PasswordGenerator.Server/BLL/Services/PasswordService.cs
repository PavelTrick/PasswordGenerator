using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using PasswordGenerator.Server.DAL;
using PasswordGenerator.Server.DAL.Models;
using PasswordGenerator.Server.Models;
using System.Diagnostics;
using System.Linq;

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

        public async Task<List<String>> TakePasswords(PasswordRequest passwordRequest, string userId)
        {
            var dateNow = DateTime.UtcNow;
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            int userPasswordCount = _context.Users
                .Include(u => u.UserPasswords)
                .ThenInclude(up => up.Password)
                .FirstOrDefault(u => u.Id == userId)?.UserPasswords?.Count() ?? 0;
            var passwordCount = _context.Passwords.Count();
            var countDifference = passwordCount - userPasswordCount;
            Boolean needGenerate = countDifference < passwordRequest.Amount;

            if (needGenerate)
            {
                await _generatorService.GeneratePasswords(passwordRequest, passwordRequest.Amount - countDifference);
            }

            List<Password> newPasswords = _context.Passwords.Skip(userPasswordCount).Take(passwordRequest.Amount).ToList();
            user.UserPasswords.AddRange(newPasswords.Select(newPassword => new UserPassword { UserId = user.Id, PasswordId = newPassword.Id }).ToList());
            await _context.SaveChangesAsync();

            return newPasswords.Select(p => p.Code).ToList();
        }

        public List<Password> GetAll()
        {
            return _context.Passwords.OrderByDescending(password => password.CreatedAt).ToList();
        }

        public List<Password> GetUserPasswords(string userId)
        {
            var passwords = _context.Users
                .Include(u => u.UserPasswords)
                .ThenInclude(up => up.Password)
                .Where(u => u.Id == userId)
                .SelectMany(u => u.UserPasswords.Select(up => up.Password))
                .OrderByDescending(password => password.CreatedAt)
                .Take(1000)
                .ToList();

            return passwords.Count > 0 ? passwords : new List<Password>();
        }

        public Statistic GetPasswordsStatistic()
        {
            Statistic statistic = new Statistic();

            statistic.TotalCount = _context.Passwords.Count();
            statistic.DuplicateCount = _context.Passwords
                .GroupBy(password => password.Code)
                .Where(group => group.Count() > 1)
                .Count();

            return statistic;
        }

        public async Task<bool> ClearUserPasswords(string userId)
        {
            var user = _context.Users
                .Include(u => u.UserPasswords)
                .ThenInclude(up => up.Password)
                .FirstOrDefault(u => u.Id == userId);

            if (user != null)
            {
                user.UserPasswords = new List<UserPassword>();
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> ClearPasswordStore()
        {
            var allPasswords = _context.Passwords.ToList();
            _context.Passwords.RemoveRange(allPasswords);

            var statistic = _context.GenerateStatistics;
            _context.GenerateStatistics.RemoveRange(statistic);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task GeneratePasswords(PasswordRequest passwordRequest)
        {
            await _generatorService.GeneratePasswords(passwordRequest);
        }

        public List<GenerateStatistic> GetGenerateLogs()
        {
            return _context.GenerateStatistics
                .Include(statistic => statistic.StatisticIterations)
                ?.OrderByDescending(password => password.Id)
                ?.Take(100)
                ?.ToList() ?? new List<GenerateStatistic>();
        }
    }
}
