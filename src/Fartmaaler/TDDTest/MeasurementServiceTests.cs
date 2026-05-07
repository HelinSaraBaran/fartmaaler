using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using FartmaalerAPI.Services;

namespace TDDTest
{
    public class MeasurementServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly MeasurementsRepo _repo;
        private readonly MeasurementService _service;

        public MeasurementServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repo = new MeasurementsRepo(_context);
            _service = new MeasurementService(_context, _repo);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public void CreateMeasurement_ReturnsNull_WhenSessionIdInvalid()
        {
            // Arrange
            int invalidSessionId = 0;

            // Act
            var result = _service.CreateMeasurement(invalidSessionId, 1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void CreateMeasurement_ReturnsNull_WhenTimeInvalid()
        {
            // Arrange
            var session = new Session
            {
                Id = 1,
                GroupId = 1,
                Group = new Group { Id = 1, Name = "G", School = "S", IsLocked = false },
                CarType = "A",
                RoadType = "R",
                SpeedLimit = 50,
                ScalingFactor = 1.0,
                Status = "running",
                CreatedAt = DateTime.UtcNow
            };
            _context.Sessions.Add(session);
            _context.SaveChanges();

            // Act
            var result = _service.CreateMeasurement(1, 0);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void CreateMeasurement_ReturnsNull_WhenSessionNotFound()
        {
            // Arrange
            // no session added

            // Act
            var result = _service.CreateMeasurement(999, 1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void CreateMeasurement_ReturnsNull_WhenSessionEnded()
        {
            // Arrange
            var session = new Session
            {
                Id = 10,
                GroupId = 1,
                Group = new Group { Id = 1, Name = "G", School = "S", IsLocked = false },
                CarType = "A",
                RoadType = "R",
                SpeedLimit = 50,
                ScalingFactor = 1.0,
                Status = "ended",
                CreatedAt = DateTime.UtcNow,
                EndedAt = DateTime.UtcNow
            };
            _context.Sessions.Add(session);
            _context.SaveChanges();

            // Act
            var result = _service.CreateMeasurement(10, 1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void CreateMeasurement_CreatesMeasurement_WithExpectedValues()
        {
            // Arrange
            // If timeSeconds = 1 -> measuredSpeedKmh = 18.00, simulated = 18/18 * SpeedLimit
            var session = new Session
            {
                Id = 5,
                GroupId = 1,
                Group = new Group { Id = 1, Name = "G", School = "S", IsLocked = false },
                CarType = "A",
                RoadType = "R",
                SpeedLimit = 50,
                ScalingFactor = 1.0,
                Status = "running",
                CreatedAt = DateTime.UtcNow
            };
            _context.Sessions.Add(session);
            _context.SaveChanges();

            // Act
            var created = _service.CreateMeasurement(5, 1);

            // Assert
            Assert.NotNull(created);
            Assert.Equal(5, created.SessionId);
            Assert.Equal(5.0, created.Distance);
            Assert.Equal(1, created.Time);
            Assert.Equal(18.00, created.MeasuredSpeed);
            Assert.Equal(50.00, created.SimulatedSpeed);
            Assert.Equal(50, created.SpeedLimit);
            Assert.Equal("On limit", created.Status);

            var fromDb = _context.Measurements.Find(created.Id);
            Assert.NotNull(fromDb);
        }
    }
}           
