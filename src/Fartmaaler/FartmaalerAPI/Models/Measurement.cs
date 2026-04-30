namespace FartmaalerAPI.Models;

public class Measurement
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    
    public double Distance { get; set; }
    public double Speed { get; set; }
    public double Time { get; set; }
    
    public string RoadType  { get; set; }
    public string CarType  { get; set; }
    
    public double Co2 { get; set; }
    public DateTime Date { get; set; }
}