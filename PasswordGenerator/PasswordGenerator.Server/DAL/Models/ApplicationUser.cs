using Microsoft.AspNetCore.Identity;

namespace PasswordGenerator.Server.DAL.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<UserPassword> UserPasswords { get; set; } = new List<UserPassword>();
    } 
}
