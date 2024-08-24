using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Server.DAL.Models;

namespace PasswordGenerator.Server.DAL
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Password> Passwords { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Password>()
               .HasOne(p => p.User)
               .WithMany() 
               .HasForeignKey(p => p.UserIdentifier)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
