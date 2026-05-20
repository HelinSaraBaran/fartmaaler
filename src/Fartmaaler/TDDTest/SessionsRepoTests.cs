using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FartmaalerAPI.Tests
{
    public class SessionsRepoTests
    {
        // Opretter fake in-memory database
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        // Tester at GetAll returnerer alle sessions
        public void GetAll_ReturnsAllSessions()
        {
            using var context = GetDbContext();

            context.Sessions.AddRange(
                new Session
                {
                    Id = 1,
                    GroupId = 1,
                    CarType = "diesel",
                    RoadType = "byzone 50",
                    SpeedLimit = 50,
                    ScalingFactor = 20,
                    Status = "Active",
                    CreatedAt = DateTime.Now
                },
                new Session
                {
                    Id = 2,
                    GroupId = 2,
                    CarType = "hybrid",
                    RoadType = "landevej 80",
                    SpeedLimit = 80,
                    ScalingFactor = 32,
                    Status = "Ended",
                    CreatedAt = DateTime.Now
                });

            context.SaveChanges();

            var repo = new SessionsRepo(context);

            var result = repo.GetAll();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        // Tester at GetById returnerer session hvis den findes
        public void GetById_ReturnsSession_WhenSessionExists()
        {
            using var context = GetDbContext();

            context.Sessions.Add(new Session
            {
                Id = 1,
                GroupId = 1,
                CarType = "diesel",
                RoadType = "byzone 50",
                SpeedLimit = 50,
                ScalingFactor = 20,
                Status = "Active",
                CreatedAt = DateTime.Now
            });

            context.SaveChanges();

            var repo = new SessionsRepo(context);

            var result = repo.GetById(1);

            Assert.NotNull(result);
            Assert.Equal("diesel", result.CarType);
        }

        [Fact]
        // Tester at GetById returnerer null hvis session ikke findes
        public void GetById_ReturnsNull_WhenSessionDoesNotExist()
        {
            using var context = GetDbContext();

            var repo = new SessionsRepo(context);

            var result = repo.GetById(1);

            Assert.Null(result);
        }

        [Fact]
        // Tester at Add tilføjer session korrekt
        public void Add_AddsSession()
        {
            using var context = GetDbContext();

            var repo = new SessionsRepo(context);

            var session = new Session
            {
                GroupId = 1,
                CarType = "diesel",
                RoadType = "byzone 50",
                SpeedLimit = 50,
                ScalingFactor = 20,
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            var result = repo.Add(session);

            Assert.NotNull(result);
            Assert.Single(context.Sessions);
        }

        [Fact]
        // Tester at Delete returnerer null hvis session ikke findes
        public void Delete_ReturnsNull_WhenSessionDoesNotExist()
        {
            using var context = GetDbContext();

            var repo = new SessionsRepo(context);

            var result = repo.Delete(1);

            Assert.Null(result);
        }

        [Fact]
        // Tester at Delete sletter session korrekt
        public void Delete_RemovesSession()
        {
            using var context = GetDbContext();

            context.Sessions.Add(new Session
            {
                Id = 1,
                GroupId = 1,
                CarType = "diesel",
                RoadType = "byzone 50",
                SpeedLimit = 50,
                ScalingFactor = 20,
                Status = "Active",
                CreatedAt = DateTime.Now
            });

            context.SaveChanges();

            var repo = new SessionsRepo(context);

            var result = repo.Delete(1);

            Assert.NotNull(result);
            Assert.Empty(context.Sessions);
        }

        [Fact]
        // Tester at Update returnerer null hvis session ikke findes
        public void Update_ReturnsNull_WhenSessionDoesNotExist()
        {
            using var context = GetDbContext();

            var repo = new SessionsRepo(context);

            var updatedSession = new Session
            {
                GroupId = 2,
                CarType = "hybrid",
                RoadType = "landevej 80",
                SpeedLimit = 80,
                ScalingFactor = 32,
                Status = "Ended"
            };

            var result = repo.Update(1, updatedSession);

            Assert.Null(result);
        }

        [Fact]
        // Tester at Update opdaterer session korrekt
        public void Update_UpdatesSession()
        {
            using var context = GetDbContext();

            context.Sessions.Add(new Session
            {
                Id = 1,
                GroupId = 1,
                CarType = "diesel",
                RoadType = "byzone 50",
                SpeedLimit = 50,
                ScalingFactor = 20,
                Status = "Active",
                CreatedAt = DateTime.Now
            });

            context.SaveChanges();

            var repo = new SessionsRepo(context);

            var updatedSession = new Session
            {
                GroupId = 2,
                CarType = "hybrid",
                RoadType = "landevej 80",
                SpeedLimit = 80,
                ScalingFactor = 32,
                Status = "Ended",
                EndedAt = DateTime.Now
            };

            var result = repo.Update(1, updatedSession);

            Assert.NotNull(result);
            Assert.Equal("hybrid", result.CarType);
            Assert.Equal("landevej 80", result.RoadType);
            Assert.Equal(80, result.SpeedLimit);
            Assert.Equal("Ended", result.Status);
        }
    }
}