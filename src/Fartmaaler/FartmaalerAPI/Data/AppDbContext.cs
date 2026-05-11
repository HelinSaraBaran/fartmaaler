using Microsoft.EntityFrameworkCore;
using FartmaalerAPI.Models;

namespace FartmaalerAPI.Data
{
    // DbContext styrer forbindelsen mellem programmet og databasen
    // Klassen indeholder alle tabeller i systemet
    public class AppDbContext : DbContext
    {
        // Constructor modtager database indstillinger
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Brugere i systemet
        public DbSet<User> Users { get; set; }

        // Grupper eller klasser
        public DbSet<Group> Groups { get; set; }

        // Sessions for gruppernes forsøg
        public DbSet<Session> Sessions { get; set; }

        // Målinger fra fartmåleren
        public DbSet<Measurement> Measurements { get; set; }

        // Hardcoded skoledata til skole leaderboard
        public DbSet<SchoolLeaderboardMock> SchoolLeaderboardMocks { get; set; }

        // Globale system indstillinger
        public DbSet<Settings> Settings { get; set; }
    }
}