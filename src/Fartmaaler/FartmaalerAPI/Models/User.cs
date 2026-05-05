namespace FartmaalerAPI.Models
{
    // Denne klasse repræsenterer en bruger i systemet
    // Bruges til login, authentication og rolle styring
    public class User
    {
        // Unik identifikator for brugeren (primary key)
        public int Id { get; set; }

        // Brugernavn som bruges til login
        public string Username { get; set; }

        // Hashet password (ikke plaintext!)
        // Bruges til sikker login validering
        public string PasswordHash { get; set; }

        // Brugerens rolle (fx "Admin" eller "User")
        // Bruges til authorization i systemet
        public string Role { get; set; }
    }
}