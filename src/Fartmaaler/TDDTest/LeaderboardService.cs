using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FartmaalerAPI.Tests
{
    public class LeaderboardServiceTests
    {
        // Opretter en fake in-memory database til tests
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        // Tester at class leaderboard returnerer resultater
        public void GetClassLeaderboard_ReturnsLeaderboardEntries()
        {
            using var context = GetDbContext();

            // Opretter gruppe
            context.Groups.Add(new Group
            {
                Id = 1,
                Name = "Gruppe 1",
                School = "Roskilde Skole"
            });

            // Opretter afsluttet session
            context.Sessions.Add(new Session
            {
                Id = 1,
                GroupId = 1,
                CarType = "benzin lille",
                RoadType = "byzone 50",
                SpeedLimit = 50,
                Status = "Ended",
                CreatedAt = DateTime.Now
            });

            // Opretter måling
            context.Measurements.Add(new Measurement
            {
                Id = 1,
                SessionId = 1,
                SimulatedSpeed = 55,
                Co2 = 5
            });

            context.SaveChanges();

            var service = new LeaderboardService(context);

            var result = service.GetClassLeaderboard("byzone 50");

            Assert.Single(result);
        }

        [Fact]
        // Tester at leaderboard bliver sorteret efter laveste score
        public void GetClassLeaderboard_ReturnsSortedScores()
        {
            using var context = GetDbContext();

            context.Groups.AddRange(
                new Group
                {
                    Id = 1,
                    Name = "Gruppe 1",
                    School = "Skole A"
                },
                new Group
                {
                    Id = 2,
                    Name = "Gruppe 2",
                    School = "Skole B"
                });

            context.Sessions.AddRange(
                new Session
                {
                    Id = 1,
                    GroupId = 1,
                    CarType = "diesel", 
                    RoadType = "byzone 50",
                    SpeedLimit = 50,
                    Status = "Ended",
                    CreatedAt = DateTime.Now
                },
                new Session
                {
                    Id = 2,
                    GroupId = 2,
                    CarType = "benzin stor",
                    RoadType = "byzone 50",
                    SpeedLimit = 50,
                    Status = "Ended",
                    CreatedAt = DateTime.Now
                });

            context.Measurements.AddRange(
                new Measurement
                {
                    SessionId = 1,
                    SimulatedSpeed = 50,
                    Co2 = 1
                },
                new Measurement
                {
                    SessionId = 2,
                    SimulatedSpeed = 90,
                    Co2 = 10
                });

            context.SaveChanges();

            var service = new LeaderboardService(context);

            var result = service.GetClassLeaderboard("byzone 50");

            Assert.Equal("Gruppe 1", result.First().GroupName);
        }

        [Fact]
        // Tester at sessioner uden målinger bliver ignoreret
        public void GetClassLeaderboard_IgnoresSessionsWithoutMeasurements()
        {
            using var context = GetDbContext();

            context.Groups.Add(new Group
            {
                Id = 1,
                Name = "Gruppe 1",
                School = "Roskilde Skole"
            });

            context.Sessions.Add(new Session
            {
                Id = 1,
                GroupId = 1,
                CarType = "hybrid",
                RoadType = "byzone 50",
                SpeedLimit = 50,
                Status = "Ended",
                CreatedAt = DateTime.Now
            });

            context.SaveChanges();

            var service = new LeaderboardService(context);

            var result = service.GetClassLeaderboard("byzone 50");

            Assert.Empty(result);
        }

        [Fact]
        // Tester at school leaderboard returnerer egen skole
        public void GetSchoolLeaderboard_ReturnsOwnSchool()
        {
            using var context = GetDbContext();

            context.Groups.Add(new Group
            {
                Id = 1,
                Name = "Gruppe 1",
                School = "Roskilde Skole"
            });

            context.Sessions.Add(new Session
            {
                Id = 1,
                GroupId = 1,
                CarType = "benzin lille",   
                RoadType = "byzone 50",
                SpeedLimit = 50,
                Status = "Ended",
                CreatedAt = DateTime.Now
            });

            context.Measurements.Add(new Measurement
            {
                SessionId = 1,
                SimulatedSpeed = 50,
                Co2 = 2
            });

            context.SaveChanges();

            var service = new LeaderboardService(context);

            var result = service.GetSchoolLeaderboard("byzone 50");

            Assert.Contains(result, school => school.IsOwnSchool);
        }

        [Fact]
        // Tester at mock skoler bliver tilføjet til leaderboard
        public void GetSchoolLeaderboard_ReturnsMockSchools()
        {
            using var context = GetDbContext();

            context.SchoolLeaderboardMocks.Add(new SchoolLeaderboardMock
            {
                Id = 1,
                SchoolName = "Test Skole",
                RoadType = "byzone 50",
                AverageScore = 10,
                AverageCo2 = 5,
                MeasurementCount = 20
            });

            context.SaveChanges();

            var service = new LeaderboardService(context);

            var result = service.GetSchoolLeaderboard("byzone 50");

            Assert.Contains(result, school => school.SchoolName == "Test Skole");
        }

        [Fact]
        // Tester at leaderboard setting returnerer false hvis setting ikke findes
        public void IsLeaderboardEnabled_ReturnsFalse_WhenSettingDoesNotExist()
        {
            using var context = GetDbContext();

            var service = new LeaderboardService(context);

            var result = service.IsLeaderboardEnabled();

            Assert.False(result);
        }

        [Fact]
        // Tester at leaderboard setting returnerer true når setting er enabled
        public void IsLeaderboardEnabled_ReturnsTrue_WhenEnabled()
        {
            using var context = GetDbContext();

            context.Settings.Add(new Settings
            {
                Key = "Leaderboard",
                Value = true
            });

            context.SaveChanges();

            var service = new LeaderboardService(context);

            var result = service.IsLeaderboardEnabled();

            Assert.True(result);
        }

        [Fact]
        // Tester at UpdateLeaderboardSetting opretter ny setting
        public void UpdateLeaderboardSetting_CreatesSetting_WhenItDoesNotExist()
        {
            using var context = GetDbContext();

            var service = new LeaderboardService(context);

            var result = service.UpdateLeaderboardSetting(true);

            Assert.NotNull(result);
            Assert.True(result.Value);
        }

        [Fact]
        // Tester at eksisterende setting bliver opdateret
        public void UpdateLeaderboardSetting_UpdatesExistingSetting()
        {
            using var context = GetDbContext();

            context.Settings.Add(new Settings
            {
                Key = "Leaderboard",
                Value = false
            });

            context.SaveChanges();

            var service = new LeaderboardService(context);

            var result = service.UpdateLeaderboardSetting(true);

            Assert.True(result.Value);
        }
    }
}