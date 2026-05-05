namespace FartmaalerAPI.DTOs
{
    // Denne klasse bruges til at modtage login data fra frontend
    // Den indeholder kun de nødvendige oplysninger: brugernavn og password
    public class LoginRequest
    {
        // Brugernavn sendt fra brugeren (fx fra en login-form)
        public string Username { get; set; }

        // Password sendt fra brugeren
        // Bliver senere verificeret mod det hash, der ligger i databasen
        public string Password { get; set; }
    }
}