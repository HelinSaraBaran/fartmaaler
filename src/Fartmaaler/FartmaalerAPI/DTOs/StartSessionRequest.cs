namespace FartmaalerAPI.DTOs
{
    public class StartSessionRequest
    {
        public int GroupId { get; set; }
        public string CarType { get; set; }
        public string RoadType { get; set; }
    }
}