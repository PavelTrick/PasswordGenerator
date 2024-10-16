namespace PasswordGenerator.Server.DAL.Models
{
    public class UserPassword
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int PasswordId { get; set; }
        public Password Password { get; set; }
    }
}
