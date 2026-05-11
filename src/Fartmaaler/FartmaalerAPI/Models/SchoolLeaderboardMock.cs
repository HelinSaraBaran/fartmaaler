namespace FartmaalerAPI.Models
{
    // Denne klasse gemmer hardcoded skoledata til leaderboard
    public class SchoolLeaderboardMock
    {
        public int Id { get; set; }

        public string SchoolName { get; set; }

        public string RoadType { get; set; }

        public double AverageScore { get; set; }

        public double AverageCo2 { get; set; }

        public int MeasurementCount { get; set; }
    }
}