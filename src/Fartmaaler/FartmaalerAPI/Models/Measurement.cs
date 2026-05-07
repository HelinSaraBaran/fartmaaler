namespace FartmaalerAPI.Models
{
    public class Measurement
    {
        public int Id { get; set; }

        public int SessionId { get; set; }

        public Session? Session { get; set; }

        public double MeasuredSpeed { get; set; }

        public double SimulatedSpeed { get; set; }

        public double Time { get; set; }

        public double Distance { get; set; }

        public int SpeedLimit { get; set; }

        public string Status { get; set; } = string.Empty;

        public double Co2 { get; set; }

        public double Co2Saved { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}