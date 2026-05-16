namespace FartmaalerAPI.DTOs
{
    public class AdminSessionResponse
    {
        public int SessionId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string CarType { get; set; } = string.Empty;
        public string RoadType { get; set; } = string.Empty;
        public double AverageSpeed { get; set; }
    }
}