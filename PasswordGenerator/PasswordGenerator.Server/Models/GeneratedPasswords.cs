using PasswordGenerator.Server.DAL.Models;

namespace PasswordGenerator.Server.Models
{
    public class GeneratedPasswords
    {
        public long GenerateTime { get; set; }
        public long ExecutionTime { get; set; }
        public List<Password> Passwords { get; set; }
        public List<GenerateStatisticIteration> Statistics { get; set; } = new List<GenerateStatisticIteration>();
    }
}
