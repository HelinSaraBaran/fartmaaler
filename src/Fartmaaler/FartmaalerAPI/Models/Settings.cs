namespace FartmaalerAPI.Models
{
    // Denne klasse gemmer globale systemindstillinger
    public class Settings
    {
        public int Id { get; set; }

        public string Key { get; set; } = string.Empty;

        public bool Value { get; set; }
    }
}