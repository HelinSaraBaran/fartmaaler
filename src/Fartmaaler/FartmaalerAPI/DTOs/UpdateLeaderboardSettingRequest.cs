namespace FartmaalerAPI.DTOs
{
    // Denne DTO bruges når admin slår leaderboard til eller fra
    public class UpdateLeaderboardSettingRequest
    {
        public bool IsEnabled { get; set; }
    }
}