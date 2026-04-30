namespace FartmaalerAPI.Models;

public class Sessions
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string CarType { get; set; }
    public string RoadType { get; set; }
    public bool IsActive { get; set; }
}