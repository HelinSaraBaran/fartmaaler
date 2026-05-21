using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using FartmaalerAPI.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FartmaalerAPI.Tests
{
    public class MeasurementServiceTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public void CreateMeasurement_ReturnsNull_WhenSessionIdIsInvalid()
        {
            using var context = GetDbContext();
            var repo = new MeasurementsRepo(context);
            var service = new MeasurementService(context, repo);

            var result = service.CreateMeasurement(0, 2);

            Assert.Null(result);
        }

        [Fact]
        public void CreateMeasurement_ReturnsNull_WhenTimeIsInvalid()
        {
            using var context = GetDbContext();
            var repo = new MeasurementsRepo(context);
            var service = new MeasurementService(context, repo);

            var result = service.CreateMeasurement(1, 0);

            Assert.Null(result);
        }

        [Fact]
        public void CreateMeasurement_ReturnsNull_WhenSessionDoesNotExist()
        {
            using var context = GetDbContext();
            var repo = new MeasurementsRepo(context);
            var service = new MeasurementService(context, repo);

            var result = service.CreateMeasurement(999, 2);

            Assert.Null(result);
        }

        [Fact]
        public void CreateMeasurement_ReturnsNull_WhenSessionIsEnded()
        {
            using var context = GetDbContext();

            context.Sessions.Add(new Session
            {
                Id = 1,
                CarType = "benzin lille",
                RoadType = "Byzone",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = "Ended"
            });

            context.SaveChanges();

            var repo = new MeasurementsRepo(context);
            var service = new MeasurementService(context, repo);

            var result = service.CreateMeasurement(1, 2);

            Assert.Null(result);
        }

        [Fact]
        public void CreateMeasurement_CreatesMeasurement_WhenInputIsValid()
        {
            using var context = GetDbContext();

            context.Sessions.Add(new Session
            {
                Id = 1,
                CarType = "benzin lille",
                RoadType = "Byzone",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = "Active"
            });

            context.SaveChanges();

            var repo = new MeasurementsRepo(context);
            var service = new MeasurementService(context, repo);

            var result = service.CreateMeasurement(1, 2);

            Assert.NotNull(result);
            Assert.Equal(1, result.SessionId);
            Assert.Equal(0.92, result.Distance, 2);
            Assert.Equal(2, result.Time);
            Assert.Equal(1.66, result.MeasuredSpeed, 2);
            Assert.Equal(16.56, result.SimulatedSpeed, 2);
            Assert.Equal(50, result.SpeedLimit);
            Assert.Equal("Too slow", result.Status);
            Assert.Equal(1.99, result.Co2, 2);
            Assert.Equal(4.01, result.Co2Saved, 2);
        }

        [Fact]
        public void CreateMeasurement_SetsStatusTooSlow_WhenSpeedIsBelowLimit()
        {
            using var context = GetDbContext();

            context.Sessions.Add(new Session
            {
                Id = 1,
                CarType = "hybrid",
                RoadType = "Byzone",
                SpeedLimit = 50,
                ScalingFactor = 1,
                Status = "Active"
            });

            context.SaveChanges();

            var repo = new MeasurementsRepo(context);
            var service = new MeasurementService(context, repo);

            var result = service.CreateMeasurement(1, 2);

            Assert.NotNull(result);
            Assert.Equal("Too slow", result.Status);
        }

        [Fact]
        public void CreateMeasurement_SavesMeasurementToDatabase()
        {
            using var context = GetDbContext();

            context.Sessions.Add(new Session
            {
                Id = 1,
                CarType = "diesel",
                RoadType = "Landevej",
                SpeedLimit = 80,
                ScalingFactor = 10,
                Status = "Active"
            });

            context.SaveChanges();

            var repo = new MeasurementsRepo(context);
            var service = new MeasurementService(context, repo);

            service.CreateMeasurement(1, 2);

            Assert.Single(context.Measurements);
        }
    }
}