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
        public LeaderboardControllerTest()
        {
            DbContextOptions<AppDbContext> options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _service = new LeaderboardService(_context);
            _controller = new LeaderboardController(_service);
        }

        // Rydder den falske database efter hver test
        public void Dispose()
        {
            _context.Dispose();
        }

        // Opretter en testgruppe
        private Group CreateGroup()
        {
            return new Group
            {
                Name = "Test Gruppe",
                School = "Køge Skole",
                IsLocked = false
            };
        }

        // Opretter en afsluttet test session
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

        // Opretter en test måling
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

        // Opretter leaderboard setting
        private void CreateLeaderboardSetting(bool value)
        {
            Settings setting = new Settings
            {
                Key = "Leaderboard",
                Value = value
            };

            _context.Settings.Add(setting);
            _context.SaveChanges();
        }

        [Fact]
        public void GetAdminClassLeaderboard_WhenRoadTypeMissing_ReturnsBadRequest()
        {
            IActionResult result = _controller.GetAdminClassLeaderboard("");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void GetAdminClassLeaderboard_WhenNoData_ReturnsNotFound()
        {
            IActionResult result = _controller.GetAdminClassLeaderboard("byzone 50");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void GetAdminClassLeaderboard_WhenDataExists_ReturnsOk()
        {
            Group group = CreateGroup();

            _context.Groups.Add(group);
            _context.SaveChanges();

            Session session = CreateSession(group.Id);

            _context.Sessions.Add(session);
            _context.SaveChanges();

            Measurement measurement = CreateMeasurement(session.Id);

            _context.Measurements.Add(measurement);
            _context.SaveChanges();

            IActionResult result = _controller.GetAdminClassLeaderboard("byzone 50");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void GetAdminSchoolLeaderboard_WhenMockDataExists_ReturnsOk()
        {
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

            IActionResult result = _controller.GetAdminSchoolLeaderboard("byzone 50");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void GetStudentClassLeaderboard_WhenLeaderboardDisabled_ReturnsForbid()
        {
            CreateLeaderboardSetting(false);

            IActionResult result = _controller.GetStudentClassLeaderboard("byzone 50");

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void GetStudentSchoolLeaderboard_WhenLeaderboardEnabled_ReturnsOk()
        {
            CreateLeaderboardSetting(true);

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

            IActionResult result = _controller.GetStudentSchoolLeaderboard("byzone 50");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void UpdateLeaderboardSetting_WhenCalled_ReturnsOk()
        {
            UpdateLeaderboardSettingRequest request =
                new UpdateLeaderboardSettingRequest
                {
                    IsEnabled = true
                };

            IActionResult result = _controller.UpdateLeaderboardSetting(request);

            Assert.IsType<OkObjectResult>(result);
        }
    }
}