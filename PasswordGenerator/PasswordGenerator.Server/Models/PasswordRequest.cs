namespace PasswordGenerator.Server.Models
{
    public class PasswordRequest
    {
        public int Amount { get; set; }
        public int Length { get; set; }
        public bool IncludeSpecial { get; set; }
        public bool IncludeNumbers { get; set; }
        public bool IncludeUppercase { get; set; }
        public bool IncludeLowercase { get; set; }
        public double ExpiredIn { get; set; }
        public int UserId { get; set; }
    }
}
