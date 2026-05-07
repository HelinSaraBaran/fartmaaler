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
    // Denne testklasse tester SessionService
    // xUnit bruges til at teste om servicen virker korrekt
    public class SessionServiceTest : IDisposable
    {
        // InMemory database bruges så vi ikke rammer rigtig database
        private readonly AppDbContext _context;

        // Servicen der skal testes
        private readonly SessionService _sessionService;

        // Constructor kører før hver test
        public SessionServiceTest()
        {
            // Opretter fake database i hukommelsen
            DbContextOptions<AppDbContext> options =
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;

            // Opretter DbContext
            _context = new AppDbContext(options);

            // Opretter service
            _sessionService = new SessionService(_context);
        }

        // Dispose rydder op efter hver test
        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public void EndSession_WhenSessionDoesNotExist_ReturnsNull()
        {
            // Act
            // Prøver at afslutte session der ikke findes
            Session? result = _sessionService.EndSession(999);

            // Assert
            // Tester at resultat er null
            Assert.Null(result);
        }

        [Fact]
        public void EndSession_WhenSessionExists_ChangesStatusToEnded()
        {
            // Arrange
            // Opretter gruppe
            Group group = new Group
            {
                Name = "Gruppe 1",
                School = "Zealand",
                IsLocked = true
            };

            // Gemmer gruppe først så EF Core laver Id
            _context.Groups.Add(group);
            _context.SaveChanges();

            // Opretter session
            Session session = new Session
            {
                GroupId = group.Id,
                CarType = "Toy car",
                RoadType = "Asphalt",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = "Started",
                CreatedAt = DateTime.Now,
                EndedAt = null
            };

            // Gemmer session
            _context.Sessions.Add(session);
            _context.SaveChanges();

            // Act
            // Afslutter session
            Session? result = _sessionService.EndSession(session.Id);

            // Assert
            // Tjekker at session findes
            Assert.NotNull(result);

            // Tjekker at status blev ændret
            Assert.Equal("Ended", result.Status);

            // Tjekker at sluttidspunkt blev sat
            Assert.NotNull(result.EndedAt);
        }

        [Fact]
        public void EndSession_WhenSessionEnds_UnlocksGroup()
        {
            // Arrange
            // Opretter gruppe
            Group group = new Group
            {
                Name = "Gruppe 1",
                School = "Zealand",
                IsLocked = true
            };

            // Gemmer gruppe
            _context.Groups.Add(group);
            _context.SaveChanges();

            // Opretter session
            Session session = new Session
            {
                GroupId = group.Id,
                CarType = "Toy car",
                RoadType = "Asphalt",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = "Started",
                CreatedAt = DateTime.Now
            };

            // Gemmer session
            _context.Sessions.Add(session);
            _context.SaveChanges();

            // Act
            // Afslutter session
            _sessionService.EndSession(session.Id);

            // Finder opdateret gruppe
            Group? updatedGroup =
                _context.Groups.FirstOrDefault(g => g.Id == group.Id);

            // Assert
            // Tjekker at gruppe findes
            Assert.NotNull(updatedGroup);

            // Tjekker at gruppen blev låst op
            Assert.False(updatedGroup.IsLocked);
        }

        [Fact]
        public void GetHistoryByGroup_WhenGroupDoesNotExist_ReturnsNull()
        {
            // Act
            // Henter historik for gruppe der ikke findes
            object? result =
                _sessionService.GetHistoryByGroup(
                    999,
                    null,
                    null,
                    null,
                    null);

            // Assert
            // Tester at resultat er null
            Assert.Null(result);
        }

        [Fact]
        public void GetHistoryByGroup_WhenGroupExists_ReturnsHistory()
        {
            // Arrange
            // Opretter gruppe
            Group group = new Group
            {
                Name = "Gruppe 1",
                School = "Zealand",
                IsLocked = false
            };

            // Gemmer gruppe
            _context.Groups.Add(group);
            _context.SaveChanges();

            // Opretter session
            Session session = new Session
            {
                GroupId = group.Id,
                CarType = "Toy car",
                RoadType = "Asphalt",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = "Ended",
                CreatedAt = DateTime.Now,
                EndedAt = DateTime.Now
            };

            // Gemmer session
            _context.Sessions.Add(session);
            _context.SaveChanges();

            // Opretter måling
            Measurement measurement = new Measurement
            {
                SessionId = session.Id,
                SimulatedSpeed = 40,
                Co2 = 20,
                Co2Saved = 10
            };

            // Gemmer måling
            _context.Measurements.Add(measurement);
            _context.SaveChanges();

            // Act
            // Henter historik
            object? result =
                _sessionService.GetHistoryByGroup(
                    group.Id,
                    null,
                    null,
                    null,
                    null);

            // Assert
            // Tjekker at historik findes
            Assert.NotNull(result);

            // Konverterer resultat til liste
            List<object> history =
                ((IEnumerable<object>)result).ToList();

            // Tjekker at der kun er én session
            Assert.Single(history);
        }

        [Fact]
        public void GetHistoryByGroup_WhenFilteringByCarType_ReturnsCorrectSessions()
        {
            // Arrange
            // Opretter gruppe
            Group group = new Group
            {
                Name = "Gruppe 1",
                School = "Zealand",
                IsLocked = false
            };

            // Gemmer gruppe
            _context.Groups.Add(group);
            _context.SaveChanges();

            // Session med toy car
            Session toyCarSession = new Session
            {
                GroupId = group.Id,
                CarType = "Toy car",
                RoadType = "Asphalt",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = "Ended",
                CreatedAt = DateTime.Now
            };

            // Session med truck
            Session truckSession = new Session
            {
                GroupId = group.Id,
                CarType = "Truck",
                RoadType = "Asphalt",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = "Ended",
                CreatedAt = DateTime.Now
            };

            // Gemmer sessions
            _context.Sessions.Add(toyCarSession);
            _context.Sessions.Add(truckSession);
            _context.SaveChanges();

            // Act
            // Filtrerer kun toy car sessions
            object? result =
                _sessionService.GetHistoryByGroup(
                    group.Id,
                    "Toy car",
                    null,
                    null,
                    null);

            // Assert
            // Tjekker at resultat findes
            Assert.NotNull(result);

            // Konverterer til liste
            List<object> history =
                ((IEnumerable<object>)result).ToList();

            // Tjekker at kun en session blev fundet
            Assert.Single(history);
        }
    }
}