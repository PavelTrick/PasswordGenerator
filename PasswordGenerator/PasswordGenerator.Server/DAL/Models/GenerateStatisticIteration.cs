using System.Text.Json.Serialization;

namespace PasswordGenerator.Server.DAL.Models
{
    public class GenerateStatisticIteration
    {
        public int Id { get; set; }
        public int IterationNumber { get; set; }
        public DateTime LogTime { get; set; }
        public int DuplicationCount { get; set; }
        public long GeneratePasswordTime { get; set; }
        public long VerifyDBUniquesTime { get; set; }

        public int GenerateStatisticId { get; set; }

        [JsonIgnore]
        public GenerateStatistic GenerateStatistic { get; set; }
    }
}
