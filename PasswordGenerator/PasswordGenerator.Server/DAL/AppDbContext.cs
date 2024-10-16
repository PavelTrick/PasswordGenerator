using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Server.DAL.Models;

namespace PasswordGenerator.Server.DAL
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Password> Passwords { get; set; }
        public DbSet<UserPassword> UserPasswords { get; set; }

        public DbSet<GenerateStatistic> GenerateStatistics { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserPassword>()
                      .HasKey(up => new { up.UserId, up.PasswordId });

            modelBuilder.Entity<UserPassword>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserPasswords)
                .HasForeignKey(up => up.UserId);

            modelBuilder.Entity<UserPassword>()
                .HasOne(up => up.Password)
                .WithMany(p => p.UserPasswords)
                .HasForeignKey(up => up.PasswordId);

            modelBuilder.Entity<GenerateStatisticIteration>()
                .Property(gs => gs.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<GenerateStatistic>()
                .HasMany(gs => gs.StatisticIterations)
                .WithOne(gsi => gsi.GenerateStatistic)
                .HasForeignKey(gsi => gsi.GenerateStatisticId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
