using System;
using System.Linq;
using FartmaalerAPI.Controllers;
using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TDDTest
{
    // Tester MeasurementsController, så vi ved at endpoints og målings logik virker korrekt
    // Testene bruger en InMemory database, så vi ikke rammer den rigtige database
    public class MeasurementsControllerTest : IDisposable
    {
        // Test database context
        private readonly AppDbContext _context;

        // Repository som controlleren bruger
        private readonly MeasurementsRepo _repo;

        // Controller som vi tester
        private readonly MeasurementsController _controller;

        // Constructor kører før hver test
        public MeasurementsControllerTest()
        {
            // Opretter en unik InMemory database til hver test
            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Opretter context
            _context = new AppDbContext(options);

            // Opretter repository
            _repo = new MeasurementsRepo(_context);

            // Opretter controller
            _controller = new MeasurementsController(_repo, _context);
        }

        // Dispose rydder op efter testen
        public void Dispose()
        {
            _context.Dispose();
        }

        // Hjælpemetode - opretter session
        private Session CreateSession(string status = "Started")
        {
            return new Session
            {
                GroupId = 1,
                CarType = "Toy car",
                RoadType = "Asphalt",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = status,
                CreatedAt = DateTime.Now
            };
        }

        // Hjælpemetode - opretter measurement
        private Measurement CreateMeasurement(int sessionId)
        {
            return new Measurement
            {
                SessionId = sessionId,
                MeasuredSpeed = 40,
                SimulatedSpeed = 400,
                Time = 2,
                Distance = 20,
                Co2 = 10,
                Co2Saved = 5,
                CreatedAt = DateTime.Now
            };
        }

        #region ADD TESTS

        [Fact]
        public void Add_ValidMeasurement_ReturnsCreated()
        {
            // Arrange - opretter session
            var session = CreateSession();
            _context.Sessions.Add(session);
            _context.SaveChanges();

            // Arrange - opretter measurement
            var measurement = CreateMeasurement(session.Id);

            // Act
            var result = _controller.Add(measurement);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        [Fact]
        public void Add_WhenSessionIdInvalid_ReturnsBadRequest()
        {
            // Arrange
            var measurement = CreateMeasurement(0);

            // Act
            var result = _controller.Add(measurement);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenSessionNotFound_ReturnsNotFound()
        {
            // Arrange
            var measurement = CreateMeasurement(999);

            // Act
            var result = _controller.Add(measurement);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenSessionEnded_ReturnsBadRequest()
        {
            // Arrange - opretter afsluttet session
            var session = CreateSession("Ended");
            _context.Sessions.Add(session);
            _context.SaveChanges();

            var measurement = CreateMeasurement(session.Id);

            // Act
            var result = _controller.Add(measurement);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        #endregion

        #region DELETE TESTS

        [Fact]
        public void Delete_WhenNotExists_ReturnsNotFound()
        {
            // Act
            var result = _controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        #endregion

        #region UPDATE TESTS

        [Fact]
        public void Update_Always_ReturnsBadRequest()
        {
            // Arrange
            var measurement = CreateMeasurement(1);

            // Act
            var result = _controller.Update(1, measurement);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region SUMMARY TESTS

        [Fact]
        public void GetSessionSummary_WhenSessionNotFound_ReturnsNotFound()
        {
            // Act
            var result = _controller.GetSessionSummary(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void GetSessionSummary_WhenNoMeasurements_ReturnsZeroValues()
        {
            // Arrange
            var session = CreateSession();
            _context.Sessions.Add(session);
            _context.SaveChanges();

            // Act
            var result = _controller.GetSessionSummary(session.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        #endregion
    }
}