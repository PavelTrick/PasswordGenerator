namespace PasswordGenerator.Server.Models
{
    public class GenerateStatistic
    {
        public DateTime LogTime { get; set; }
        public int DuplicationCount { get; set; }
        public long GeneratePasswordTime { get; set; }
        public long VerifyDBUniquesTime { get; set; }
    }
}
