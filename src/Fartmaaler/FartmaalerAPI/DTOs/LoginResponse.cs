namespace FartmaalerAPI.DTOs
{
    // Denne klasse bruges til at sende login svar tilbage til frontend
    // Den indeholder det vigtigste data efter et succesfuldt login
    public class LoginResponse
    {
        // JWT token som bruges til at identificere brugeren i systemet
        // Sendes med i fremtidige requests for authentication
        public string Token { get; set; }

        // Brugernavn på den bruger der er logget ind
        public string Username { get; set; }

        // Brugerens rolle (fx Admin eller User)
        // Bruges til authorization i systemet
        public string Role { get; set; }
    }
}