using FartmaalerAPI.Controllers;
using FartmaalerAPI.Data;
using FartmaalerAPI.DTOs;
using FartmaalerAPI.Models;
using FartmaalerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Xunit;

namespace TDDTest
{
    public class LeaderboardControllerTest : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly LeaderboardService _service;
        private readonly LeaderboardController _controller;

        // Constructoren kører før hver test
        // Her laver vi en falsk database i hukommelsen
        // Så tester vi uden at bruge den rigtige database
        public LeaderboardControllerTest()
        {
            DbContextOptions<AppDbContext> options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            // Servicen får den falske database
            // Det gør at controlleren kan testes realistisk
            _service = new LeaderboardService(_context);

            // Controlleren får servicen
            // Så testen rammer samme flow som API'et
            _controller = new LeaderboardController(_service);
        }

        // Dispose kører efter hver test
        // Den rydder databasen op fra hukommelsen
        public void Dispose()
        {
            _context.Dispose();
        }

        // Hjælpemetode der opretter en testgruppe
        // Gruppen bruges til klasse leaderboard
        private Group CreateGroup()
        {
            return new Group
            {
                Name = "Test Gruppe",
                School = "Køge Skole",
                IsLocked = false
            };
        }

        // Hjælpemetode der opretter en test session
        // Status er Ended fordi leaderboard kun bruger afsluttede sessions
        private Session CreateSession(int groupId, string status = "Ended")
        {
            return new Session
            {
                GroupId = groupId,
                CarType = "benzin lille",
                RoadType = "byzone 50",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = status,
                CreatedAt = DateTime.Now,
                EndedAt = DateTime.Now
            };
        }

        // Hjælpemetode der opretter en test måling
        // Målingen bruges til at beregne score og CO2
        private Measurement CreateMeasurement(int sessionId)
        {
            return new Measurement
            {
                SessionId = sessionId,
                Distance = 5,
                Time = 1,
                MeasuredSpeed = 18,
                SimulatedSpeed = 50,
                SpeedLimit = 50,
                Status = "On limit",
                Co2 = 5,
                Co2Saved = 2,
                CreatedAt = DateTime.Now
            };
        }

        [Fact]
        public void GetAdminClassLeaderboard_WhenRoadTypeMissing_ReturnsBadRequest()
        {
            // Arrange er tom
            // Vi tester hvad der sker når roadType mangler

            // Act kalder controller metoden med tom vejtype
            IActionResult result = _controller.GetAdminClassLeaderboard("");

            // Assert tjekker at controlleren returnerer 400
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void GetAdminClassLeaderboard_WhenNoData_ReturnsNotFound()
        {
            // Arrange er tom
            // Der findes ingen sessions eller målinger i databasen

            // Act prøver at hente leaderboard for byzone 50
            IActionResult result = _controller.GetAdminClassLeaderboard("byzone 50");

            // Assert tjekker at controlleren returnerer 404
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void GetAdminClassLeaderboard_WhenDataExists_ReturnsOk()
        {
            // Arrange opretter gruppe session og måling
            Group group = CreateGroup();

            _context.Groups.Add(group);
            _context.SaveChanges();

            Session session = CreateSession(group.Id);

            _context.Sessions.Add(session);
            _context.SaveChanges();

            Measurement measurement = CreateMeasurement(session.Id);

            _context.Measurements.Add(measurement);
            _context.SaveChanges();

            // Act henter klasse leaderboard
            IActionResult result =
                _controller.GetAdminClassLeaderboard("byzone 50");

            // Assert tjekker at controlleren returnerer 200
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void GetAdminSchoolLeaderboard_WhenMockDataExists_ReturnsOk()
        {
            // Arrange opretter mock skoledata
            // Det svarer til andre skolers hardcoded data
            SchoolLeaderboardMock mock = new SchoolLeaderboardMock
            {
                SchoolName = "Roskilde Skole",
                RoadType = "byzone 50",
                AverageScore = 10,
                AverageCo2 = 5,
                MeasurementCount = 20
            };

            _context.SchoolLeaderboardMocks.Add(mock);
            _context.SaveChanges();

            // Act henter skole leaderboard
            IActionResult result =
                _controller.GetAdminSchoolLeaderboard("byzone 50");

            // Assert tjekker at controlleren returnerer 200
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void GetStudentClassLeaderboard_WhenLeaderboardDisabled_ReturnsForbid()
        {
            // Arrange slår leaderboard fra
            // Elever må derfor ikke se leaderboard
            LeaderboardSetting setting = new LeaderboardSetting
            {
                IsEnabled = false
            };

            _context.LeaderboardSettings.Add(setting);
            _context.SaveChanges();

            // Act elev prøver at hente klasse leaderboard
            IActionResult result =
                _controller.GetStudentClassLeaderboard("byzone 50");

            // Assert tjekker at controlleren returnerer 403
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void GetStudentSchoolLeaderboard_WhenLeaderboardEnabled_ReturnsOk()
        {
            // Arrange slår leaderboard til
            // Elever må derfor se leaderboard
            LeaderboardSetting setting = new LeaderboardSetting
            {
                IsEnabled = true
            };

            _context.LeaderboardSettings.Add(setting);

            // Arrange opretter mock skoledata
            SchoolLeaderboardMock mock = new SchoolLeaderboardMock
            {
                SchoolName = "Roskilde Skole",
                RoadType = "byzone 50",
                AverageScore = 10,
                AverageCo2 = 5,
                MeasurementCount = 20
            };

            _context.SchoolLeaderboardMocks.Add(mock);

            _context.SaveChanges();

            // Act elev henter skole leaderboard
            IActionResult result =
                _controller.GetStudentSchoolLeaderboard("byzone 50");

            // Assert tjekker at controlleren returnerer 200
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void UpdateLeaderboardSetting_WhenCalled_ReturnsOk()
        {
            // Arrange laver request objekt
            // Requesten fortæller at leaderboard skal slås til
            UpdateLeaderboardSettingRequest request =
                new UpdateLeaderboardSettingRequest
                {
                    IsEnabled = true
                };

            // Act kalder controller metoden
            IActionResult result =
                _controller.UpdateLeaderboardSetting(request);

            // Assert tjekker at controlleren returnerer 200
            Assert.IsType<OkObjectResult>(result);
        }
    }
}