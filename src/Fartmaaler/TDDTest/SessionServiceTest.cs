using System;
using System.Collections.Generic;
using System.Linq;
using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TDDTest
{
    public class SessionServiceTest : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly SessionService _sessionService;

        public SessionServiceTest()
        {
            DbContextOptions<AppDbContext> options =
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;

            _context = new AppDbContext(options);
            _sessionService = new SessionService(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        private Group CreateGroup(bool isLocked = false)
        {
            return new Group
            {
                Name = "Gruppe 1",
                School = "Zealand",
                IsLocked = isLocked
            };
        }

        private Session CreateSession(int groupId, string carType = "Toy car", string status = "Started")
        {
            return new Session
            {
                GroupId = groupId,
                CarType = carType,
                RoadType = "Byzone 50",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = status,
                CreatedAt = DateTime.Now
            };
        }

        [Fact]
        public void EndSession_WhenSessionDoesNotExist_ReturnsNull()
        {
            Session? result = _sessionService.EndSession(999);

            Assert.Null(result);
        }

        [Fact]
        public void EndSession_WhenSessionExists_ChangesStatusToEnded()
        {
            Group group = CreateGroup(isLocked: true);
            _context.Groups.Add(group);
            _context.SaveChanges();

            Session session = CreateSession(group.Id);
            _context.Sessions.Add(session);
            _context.SaveChanges();

            Session? result = _sessionService.EndSession(session.Id);

            Assert.NotNull(result);
            Assert.Equal("Ended", result.Status);
            Assert.NotNull(result.EndedAt);
        }

        [Fact]
        public void EndSession_WhenSessionEnds_UnlocksGroup()
        {
            Group group = CreateGroup(isLocked: true);
            _context.Groups.Add(group);
            _context.SaveChanges();

            Session session = CreateSession(group.Id);
            _context.Sessions.Add(session);
            _context.SaveChanges();

            _sessionService.EndSession(session.Id);

            Group? updatedGroup = _context.Groups.FirstOrDefault(g => g.Id == group.Id);

            Assert.NotNull(updatedGroup);
            Assert.False(updatedGroup.IsLocked);
        }

        [Fact]
        public void GetHistoryByGroup_WhenGroupDoesNotExist_ReturnsNull()
        {
            object? result = _sessionService.GetHistoryByGroup(
                999,
                null,
                null,
                null,
                null);

            Assert.Null(result);
        }

        [Fact]
        public void GetHistoryByGroup_WhenGroupExists_ReturnsHistory()
        {
            Group group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            Session session = CreateSession(group.Id, status: "Ended");
            session.EndedAt = DateTime.Now;

            _context.Sessions.Add(session);
            _context.SaveChanges();

            Measurement measurement = new Measurement
            {
                SessionId = session.Id,
                SimulatedSpeed = 40,
                Co2 = 20,
                Co2Saved = 10,
                CreatedAt = DateTime.Now
            };

            _context.Measurements.Add(measurement);
            _context.SaveChanges();

            object? result = _sessionService.GetHistoryByGroup(
                group.Id,
                null,
                null,
                null,
                null);

            Assert.NotNull(result);

            List<object> history = ((IEnumerable<object>)result).ToList();

            Assert.Single(history);
        }

        [Fact]
        public void GetHistoryByGroup_WhenFilteringByCarType_ReturnsCorrectSessions()
        {
            Group group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            Session toyCarSession = CreateSession(group.Id, carType: "Toy car", status: "Ended");
            Session truckSession = CreateSession(group.Id, carType: "Truck", status: "Ended");

            _context.Sessions.Add(toyCarSession);
            _context.Sessions.Add(truckSession);
            _context.SaveChanges();

            object? result = _sessionService.GetHistoryByGroup(
                group.Id,
                "Toy car",
                null,
                null,
                null);

            Assert.NotNull(result);

            List<object> history = ((IEnumerable<object>)result).ToList();

            Assert.Single(history);
        }
    }
}