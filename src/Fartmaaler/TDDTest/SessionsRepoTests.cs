using System;
using System.Linq;
using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TDDTest
{
    // Tester SessionsRepo
    // Bruger InMemory database, så testene ikke påvirker den rigtige database
    public class SessionsRepoTests : IDisposable
    {
        // Test database context
        private readonly AppDbContext _context;

        // Repository der testes
        private readonly SessionsRepo _repo;

        // Kører før hver test
        public SessionsRepoTests()
        {
            // Opretter en unik InMemory database til hver test
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Opretter context og repository
            _context = new AppDbContext(options);
            _repo = new SessionsRepo(_context);
        }

        // Rydder op efter hver test
        public void Dispose()
        {
            _context.Dispose();
        }

        // Hjælpemetode der opretter en standard test session
        private Session CreateSession()
        {
            return new Session
            {
                GroupId = 1,
                CarType = "Toy car",
                RoadType = "Asphalt",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = "Started",
                CreatedAt = DateTime.Now,
                EndedAt = null
            };
        }

        #region GET TESTS

        [Fact]
        public void GetAll_WhenRepoIsEmpty_ReturnsEmptyList()
        {
            // Act -  henter alle sessions fra tom database
            var result = _repo.GetAll();

            // Assert: forventer tom liste
            Assert.Empty(result);
        }

        [Fact]
        public void GetAll_WhenSessionsExist_ReturnsAllSessions()
        {
            // Arrange - tilføjer tre sessions
            _context.Sessions.AddRange(
                CreateSession(),
                CreateSession(),
                CreateSession()
            );
            _context.SaveChanges();

            // Act - henter alle sessions
            var result = _repo.GetAll().ToList();

            // Assert - forventer tre sessions
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetById_WhenIdExists_ReturnsSession()
        {
            // Arrange - opretter og gemmer en session
            var session = CreateSession();
            _context.Sessions.Add(session);
            _context.SaveChanges();

            // Act - henter session ud fra id
            var result = _repo.GetById(session.Id);

            // Assert - session findes og har korrekt id
            Assert.NotNull(result);
            Assert.Equal(session.Id, result.Id);
        }

        [Fact]
        public void GetById_WhenIdDoesNotExist_ReturnsNull()
        {
            // Act - forsøger at hente session med id der ikke findes
            var result = _repo.GetById(999);

            // Assert -forventer null
            Assert.Null(result);
        }

        #endregion

        #region ADD TESTS

        [Fact]
        public void Add_ValidSession_AddsSessionToDatabase()
        {
            // Arrange - opretter en gyldig session
            var session = CreateSession();

            // Act - tilføjer session til databasen
            var result = _repo.Add(session);

            // Assert -  session er gemt og har fået id
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Single(_context.Sessions);
        }

        [Fact]
        public void Add_ValidSession_PropertiesAreSavedCorrectly()
        {
            // Arrange - opretter en session
            var session = CreateSession();

            // Act -  gemmer sessionen
            var result = _repo.Add(session);

            // Assert -  tjekker at alle værdier er gemt korrekt
            Assert.Equal(1, result.GroupId);
            Assert.Equal("Toy car", result.CarType);
            Assert.Equal("Asphalt", result.RoadType);
            Assert.Equal(50, result.SpeedLimit);
            Assert.Equal(10, result.ScalingFactor);
            Assert.Equal("Started", result.Status);
            Assert.Null(result.EndedAt);
        }

        [Fact]
        public void Add_ValidSession_CanBeFoundById()
        {
            // Arrange - opretter en session
            var session = CreateSession();

            // Act - gemmer sessionen og henter den igen
            var added = _repo.Add(session);
            var found = _repo.GetById(added.Id);

            // Assert-  session kan findes igen
            Assert.NotNull(found);
            Assert.Equal(added.Id, found.Id);
        }

        #endregion

        #region DELETE TESTS

        [Fact]
        public void Delete_WhenIdDoesNotExist_ReturnsNull()
        {
            // Act -  forsøger at slette session der ikke findes
            var result = _repo.Delete(999);

            // Assert - forventer null
            Assert.Null(result);
        }

        [Fact]
        public void Delete_WhenIdExists_RemovesSessionFromDatabase()
        {
            // Arrange - opretter en session
            var session = _repo.Add(CreateSession());

            // Act - sletter sessionen
            var deleted = _repo.Delete(session.Id);
            var all = _repo.GetAll();

            // Assert -  session er slettet fra databasen
            Assert.NotNull(deleted);
            Assert.Empty(all);
        }

        [Fact]
        public void Delete_WhenIdExists_ReturnsDeletedSession()
        {
            // Arrange - opretter en session
            var session = _repo.Add(CreateSession());

            // Act - sletter sessionen
            var result = _repo.Delete(session.Id);

            // Assert - den slettede session returneres
            Assert.NotNull(result);
            Assert.Equal(session.Id, result.Id);
            Assert.Equal("Toy car", result.CarType);
        }

        [Fact]
        public void Delete_OneOfMultiple_RemovesOnlySelectedSession()
        {
            // Arrange - opretter to sessions
            var s1 = _repo.Add(CreateSession());
            var s2 = _repo.Add(CreateSession());

            // Act - sletter kun den første session
            _repo.Delete(s1.Id);
            var all = _repo.GetAll().ToList();

            // Assert -  kun den anden session er tilbage
            Assert.Single(all);
            Assert.Equal(s2.Id, all[0].Id);
        }

        #endregion

        #region UPDATE TESTS

        [Fact]
        public void Update_WhenIdDoesNotExist_ReturnsNull()
        {
            // Arrange - opretter nye session værdier
            var updatedSession = CreateSession();

            // Act - forsøger at opdatere en session der ikke findes
            var result = _repo.Update(999, updatedSession);

            // Assert - forventer null
            Assert.Null(result);
        }

        [Fact]
        public void Update_WhenIdExists_UpdatesSession()
        {
            // Arrange -  opretter en session
            var session = _repo.Add(CreateSession());

            // Arrange -  opretter nye værdier til sessionen
            var updatedSession = new Session
            {
                GroupId = 2,
                CarType = "Ball",
                RoadType = "Wood",
                SpeedLimit = 80,
                ScalingFactor = 5,
                Status = "Ended",
                EndedAt = DateTime.Now
            };

            // Act - opdaterer sessionen
            var result = _repo.Update(session.Id, updatedSession);

            // Assert - tjekker at felterne blev opdateret korrekt
            Assert.NotNull(result);
            Assert.Equal(2, result.GroupId);
            Assert.Equal("Ball", result.CarType);
            Assert.Equal("Wood", result.RoadType);
            Assert.Equal(80, result.SpeedLimit);
            Assert.Equal(5, result.ScalingFactor);
            Assert.Equal("Ended", result.Status);
            Assert.NotNull(result.EndedAt);
        }

        [Fact]
        public void Update_WhenIdExists_DoesNotChangeId()
        {
            // Arrange -  opretter en session
            var session = _repo.Add(CreateSession());

            // Arrange -  forsøger at sende et nyt id med i updatedSession
            var updatedSession = CreateSession();
            updatedSession.Id = 999;

            // Act -  opdaterer sessionen
            var result = _repo.Update(session.Id, updatedSession);

            // Assert - id må ikke ændres ved update
            Assert.NotNull(result);
            Assert.Equal(session.Id, result.Id);
            Assert.NotEqual(999, result.Id);
        }

        [Fact]
        public void Update_OneOfMultiple_UpdatesOnlySelectedSession()
        {
            // Arrange - opretter to sessions
            var s1 = _repo.Add(CreateSession());
            var s2 = _repo.Add(CreateSession());

            // Arrange - nye værdier til opdatering
            var updatedSession = new Session
            {
                GroupId = 99,
                CarType = "Updated",
                RoadType = "UpdatedRoad",
                SpeedLimit = 130,
                ScalingFactor = 20,
                Status = "Ended",
                EndedAt = DateTime.Now
            };

            // Act- opdaterer kun den første session
            _repo.Update(s1.Id, updatedSession);

            // Act - henter begge sessions igen
            var firstResult = _repo.GetById(s1.Id);
            var secondResult = _repo.GetById(s2.Id);

            // Assert - sikrer at begge sessions findes
            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);

            // Assert - kun den valgte session er opdateret
            Assert.Equal("Updated", firstResult.CarType);
            Assert.Equal("Toy car", secondResult.CarType);
        }

        #endregion
    }
}