using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FartmaalerAPI.Tests
{
    public class SessionServiceTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public void GetSpeedLimit_ReturnsCorrectSpeedLimit()
        {
            using var context = GetDbContext();
            var service = new SessionService(context);

            Assert.Equal(50, service.GetSpeedLimit("byzone 50"));
            Assert.Equal(80, service.GetSpeedLimit("landevej 80"));
            Assert.Equal(110, service.GetSpeedLimit("motorvej 110"));
        }

        [Fact]
        public void GetSpeedLimit_ReturnsDefault_WhenRoadTypeIsUnknown()
        {
            using var context = GetDbContext();
            var service = new SessionService(context);

            Assert.Equal(50, service.GetSpeedLimit("ukendt"));
        }

        [Fact]
        public void GetScalingFactor_ReturnsCorrectScalingFactor()
        {
            using var context = GetDbContext();
            var service = new SessionService(context);

            Assert.Equal(20, service.GetScalingFactor("byzone 50"));
            Assert.Equal(32, service.GetScalingFactor("landevej 80"));
            Assert.Equal(44, service.GetScalingFactor("motorvej 110"));
        }

        [Fact]
        public void GetScalingFactor_ReturnsDefault_WhenRoadTypeIsUnknown()
        {
            using var context = GetDbContext();
            var service = new SessionService(context);

            Assert.Equal(20, service.GetScalingFactor("ukendt"));
        }

        [Fact]
        public void StartSession_ReturnsNull_WhenGroupDoesNotExist()
        {
            using var context = GetDbContext();
            var service = new SessionService(context);

            var result = service.StartSession(999, "benzin lille", "byzone 50");

            Assert.Null(result);
        }

        [Fact]
        public void StartSession_ReturnsNull_WhenGroupIsLocked()
        {
            using var context = GetDbContext();

            context.Groups.Add(new Group
            {
                Id = 1,
                Name = "Gruppe 1",
                School = "Roskilde Skole",
                IsLocked = true
            });

            context.SaveChanges();

            var service = new SessionService(context);

            var result = service.StartSession(1, "benzin lille", "byzone 50");

            Assert.Null(result);
        }

        [Fact]
        public void StartSession_CreatesSession_WhenGroupExistsAndIsNotLocked()
        {
            using var context = GetDbContext();

            context.Groups.Add(new Group
            {
                Id = 1,
                Name = "Gruppe 1",
                School = "Roskilde Skole",
                IsLocked = false
            });

            context.SaveChanges();

            var service = new SessionService(context);

            var result = service.StartSession(1, "benzin lille", "byzone 50");

            Assert.NotNull(result);
            Assert.Equal(1, result.GroupId);
            Assert.Equal("benzin lille", result.CarType);
            Assert.Equal("byzone 50", result.RoadType);
            Assert.Equal(50, result.SpeedLimit);
            Assert.Equal(20, result.ScalingFactor);
            Assert.Equal("Active", result.Status);
            Assert.Null(result.EndedAt);
        }

        [Fact]
        public void StartSession_LocksGroup()
        {
            using var context = GetDbContext();

            context.Groups.Add(new Group
            {
                Id = 1,
                Name = "Gruppe 1",
                School = "Roskilde Skole",
                IsLocked = false
            });

            context.SaveChanges();

            var service = new SessionService(context);

            service.StartSession(1, "hybrid", "landevej 80");

            var group = context.Groups.First(g => g.Id == 1);

            Assert.True(group.IsLocked);
        }

        [Fact]
        public void StartSession_EndsExistingActiveSessions()
        {
            using var context = GetDbContext();

            context.Groups.Add(new Group
            {
                Id = 1,
                Name = "Gruppe 1",
                School = "Roskilde Skole",
                IsLocked = false
            });

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

            var service = new SessionService(context);

            service.StartSession(1, "hybrid", "landevej 80");

            var oldSession = context.Sessions.First(s => s.Id == 1);

            Assert.Equal("Ended", oldSession.Status);
            Assert.NotNull(oldSession.EndedAt);
        }

        [Fact]
        public void EndSession_ReturnsNull_WhenSessionDoesNotExist()
        {
            using var context = GetDbContext();
            var service = new SessionService(context);

            var result = service.EndSession(999);

            Assert.Null(result);
        }

        [Fact]
        public void EndSession_EndsActiveSession()
        {
            using var context = GetDbContext();

            context.Groups.Add(new Group
            {
                Id = 1,
                Name = "Gruppe 1",
                School = "Roskilde Skole",
                IsLocked = true
            });

            context.Sessions.Add(new Session
            {
                Id = 1,
                GroupId = 1,
                CarType = "benzin lille",
                RoadType = "byzone 50",
                SpeedLimit = 50,
                ScalingFactor = 20,
                Status = "Active",
                CreatedAt = DateTime.Now
            });

            context.SaveChanges();

            var service = new SessionService(context);

            var result = service.EndSession(1);

            Assert.NotNull(result);
            Assert.Equal("Ended", result.Status);
            Assert.NotNull(result.EndedAt);
        }

        [Fact]
        public void EndSession_UnlocksGroup()
        {
            using var context = GetDbContext();

            context.Groups.Add(new Group
            {
                Id = 1,
                Name = "Gruppe 1",
                School = "Roskilde Skole",
                IsLocked = true
            });

            context.Sessions.Add(new Session
            {
                Id = 1,
                GroupId = 1,
                CarType = "benzin lille",
                RoadType = "byzone 50",
                SpeedLimit = 50,
                ScalingFactor = 20,
                Status = "Active",
                CreatedAt = DateTime.Now
            });

            context.SaveChanges();

            var service = new SessionService(context);

            service.EndSession(1);

            var group = context.Groups.First(g => g.Id == 1);

            Assert.False(group.IsLocked);
        }

        [Fact]
        public void EndSession_ReturnsSession_WhenSessionIsAlreadyEnded()
        {
            using var context = GetDbContext();

            context.Sessions.Add(new Session
            {
                Id = 1,
                GroupId = 1,
                CarType = "benzin lille",
                RoadType = "byzone 50",
                SpeedLimit = 50,
                ScalingFactor = 20,
                Status = "Ended",
                CreatedAt = DateTime.Now,
                EndedAt = DateTime.Now
            });

            context.SaveChanges();

            var service = new SessionService(context);

            var result = service.EndSession(1);

            Assert.NotNull(result);
            Assert.Equal("Ended", result.Status);
        }
    }
}