namespace FartmaalerAPI.Models
{
    // Denne klasse repræsenterer en måling fra fartmåleren
    // Den indeholder alle data fra et enkelt forsøg med bilen
    public class Measurement
    {
        // Unik identifikator for målingen (primary key)
        public int Id { get; set; }

        // Foreign key der forbinder målingen til en session
        public int SessionId { get; set; }

        // Navigation property til Session
        // Gør det muligt at tilgå session data direkte fra målingen
        public Session Session { get; set; }

        // Den faktiske målte hastighed fra sensorerne (real data)
        public double MeasuredSpeed { get; set; }

        // Den simulerede hastighed efter scaling factor
        // Bruges til at gøre målingen realistisk ift  rigtig trafik
        public double SimulatedSpeed { get; set; }

        // Tiden bilen brugte mellem de to sensorer (sekunder)
        public double Time { get; set; }

        // Afstanden mellem sensorerne (meter)
        public double Distance { get; set; }

        // Beregnet CO2 udledning baseret på hastighed
        public double Co2 { get; set; }

        // Beregnet CO2 besparelse hvis man kører korrekt
        public double Co2Saved { get; set; }

        // Tidspunkt hvor målingen blev oprettet
        public DateTime CreatedAt { get; set; }
    }
}