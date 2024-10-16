namespace PasswordGenerator.Server.DAL.Models
{
    public class GenerateStatistic
    {
        public int Id { get; set; }
        public int PasswordAmount { get; set; }
        public List<GenerateStatisticIteration> StatisticIterations { get; set; } = new List<GenerateStatisticIteration>();
        public long TotalTime { get; set; }

    }
}
