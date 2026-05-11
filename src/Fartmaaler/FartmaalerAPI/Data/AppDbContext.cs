using Microsoft.EntityFrameworkCore;
using FartmaalerAPI.Models;

namespace FartmaalerAPI.Data
{
    // DbContext er forbindelsen mellem programmet og databasen
    // Denne klasse styrer hvilke tabeller vi har og hvordan vi arbejder med data
    public class AppDbContext : DbContext
    {
        // Constructor modtager database-indstillinger (connection string osv)
        // og sender dem videre til base DbContext
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Users bliver til en tabel i databasen
        // Indeholder alle brugere i systemet
        public DbSet<User> Users { get; set; }

        // Groups bliver til en tabel
        // Bruges til at gruppere brugere (fx klasser eller hold)
        public DbSet<Group> Groups { get; set; }

        // Sessions bliver til en tabel
        // En session kan være en måling eller et forsøg med fartmåleren
        public DbSet<Session> Sessions { get; set; }

        // Measurements bliver til en tabel
        // Indeholder selve målingerne (hastighed, tid osv.)
        public DbSet<Measurement> Measurements { get; set; }


        // SchoolLeaderboardMocks bliver til en tabel
        // Bruges til hardcoded skoledata i leaderboard
        public DbSet<SchoolLeaderboardMock> SchoolLeaderboardMocks { get; set; }

        // LeaderboardSettings bliver til en tabel i databasen
        public DbSet<LeaderboardSetting> LeaderboardSettings { get; set; }
    }
}