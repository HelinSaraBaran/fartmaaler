namespace FartmaalerAPI.DTOs
{
    // Denne DTO sendes tilbage til klasse leaderboard
    public class LeaderboardEntryResponse
    {
        public int GroupId { get; set; }

        public string GroupName { get; set; }

        public string School { get; set; }

        public string RoadType { get; set; }

        public int SessionId { get; set; }

        public double Score { get; set; }

        public double AverageDeviation { get; set; }

        public double AverageCo2 { get; set; }

        public int MeasurementCount { get; set; }
    }
}