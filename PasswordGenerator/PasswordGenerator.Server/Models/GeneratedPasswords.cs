namespace PasswordGenerator.Server.Models
{
    public class GeneratedPasswords
    {
        public long GenerateTime { get; set; }
        public long ExecutionTime { get; set; }
        public string[] Passwords { get; set; }
        public List<GenerateStatistic> Statistics { get; set; } = new List<GenerateStatistic>();
    }
}
