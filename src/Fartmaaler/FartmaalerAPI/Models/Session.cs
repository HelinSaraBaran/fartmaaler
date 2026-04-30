namespace FartmaalerAPI.Models
{
    public class Session
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
        public string CarType { get; set; }
        public string RoadType { get; set; }
        public int SpeedLimit { get; set; }
        public double ScalingFactor { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }
}