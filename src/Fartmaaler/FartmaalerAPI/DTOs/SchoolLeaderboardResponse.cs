namespace FartmaalerAPI.DTOs
{
    // Denne DTO sendes tilbage til skole leaderboard
    public class SchoolLeaderboardResponse
    {
        public string SchoolName { get; set; }

        public string RoadType { get; set; }

        public double AverageScore { get; set; }

        public double AverageCo2 { get; set; }

        public int MeasurementCount { get; set; }

        public bool IsOwnSchool { get; set; }
    }
}