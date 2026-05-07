namespace FartmaalerAPI.Models
{
    // Denne klasse repræsenterer en session (et forsøg)
    // En session er et samlet forløb, hvor en gruppe laver målinger med fartmåleren
    public class Session
    {
        // Unik identifikator for sessionen (primary key)
        public int Id { get; set; }

        // Foreign key der forbinder sessionen til en gruppe
        public int GroupId { get; set; }

        // Navigation property til Group
        // Gør det muligt at hente gruppe data direkte
        public Group? Group { get; set; }

        // Hvilken type bil der bruges i forsøget
        public string CarType { get; set; }

        // Hvilken type vej der simuleres (fx by, motorvej)
        public string RoadType { get; set; }

        // Hastighedsgrænsen for denne session
        public int SpeedLimit { get; set; }

        // Faktor der bruges til at omregne målt hastighed til realistisk hastighed
        public double ScalingFactor { get; set; }

        // Status på sessionen (fx "Active", "Finished")
        public string Status { get; set; }

        // Tidspunkt hvor sessionen starter
        public DateTime CreatedAt { get; set; }

        // Tidspunkt hvor sessionen slutter (kan være null hvis den stadig er aktiv)
        public DateTime? EndedAt { get; set; }
    }
}