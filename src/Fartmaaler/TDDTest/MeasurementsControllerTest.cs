using FartmaalerAPI.Controllers;
using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using FartmaalerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Xunit;

namespace TDDTest
{
    public class MeasurementsControllerTest : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly MeasurementsRepo _repo;
        private readonly MeasurementService _measurementService;
        private readonly MeasurementsController _controller;

        public MeasurementsControllerTest()
        {
            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repo = new MeasurementsRepo(_context);
            _measurementService = new MeasurementService(_context, _repo);
            _controller = new MeasurementsController(_repo, _context, _measurementService);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        private Session CreateSession(string status = "Started")
        {
            return new Session
            {
                GroupId = 1,
                CarType = "Toy car",
                RoadType = "Byzone",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = status,
                CreatedAt = DateTime.Now
            };
        }

        private Measurement CreateMeasurement(int sessionId)
        {
            return new Measurement
            {
                SessionId = sessionId,
                Time = 1,
                Distance = 5,
                MeasuredSpeed = 18,
                SimulatedSpeed = 50,
                SpeedLimit = 50,
                Status = "On limit",
                Co2 = 0,
                Co2Saved = 0,
                CreatedAt = DateTime.Now
            };
        }

        [Fact]
        public void Add_ValidMeasurement_ReturnsCreated()
        {
            var session = CreateSession();
            _context.Sessions.Add(session);
            _context.SaveChanges();

            var measurement = CreateMeasurement(session.Id);

            var result = _controller.Add(measurement);

            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        [Fact]
        public void Add_ValidMeasurement_SavesMeasurementInDatabase()
        {
            var session = CreateSession();
            _context.Sessions.Add(session);
            _context.SaveChanges();

            var measurement = CreateMeasurement(session.Id);

            _controller.Add(measurement);

            Assert.Single(_context.Measurements);
        }

        [Fact]
        public void Add_ValidMeasurement_CalculatesCorrectValues()
        {
            var session = CreateSession();
            _context.Sessions.Add(session);
            _context.SaveChanges();

            var measurement = CreateMeasurement(session.Id);

            _controller.Add(measurement);

            var savedMeasurement = _context.Measurements.First();

            Assert.Equal(5, savedMeasurement.Distance);
            Assert.Equal(1, savedMeasurement.Time);
            Assert.Equal(18, savedMeasurement.MeasuredSpeed);
            Assert.Equal(50, savedMeasurement.SimulatedSpeed);
            Assert.Equal(50, savedMeasurement.SpeedLimit);
            Assert.Equal("On limit", savedMeasurement.Status);
        }

        [Fact]
        public void Add_WhenSessionIdInvalid_ReturnsBadRequest()
        {
            var measurement = CreateMeasurement(0);

            var result = _controller.Add(measurement);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenSessionNotFound_ReturnsNotFound()
        {
            var measurement = CreateMeasurement(999);

            var result = _controller.Add(measurement);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenTimeIsZero_ReturnsBadRequest()
        {
            var session = CreateSession();
            _context.Sessions.Add(session);
            _context.SaveChanges();

            var measurement = CreateMeasurement(session.Id);
            measurement.Time = 0;

            var result = _controller.Add(measurement);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenSessionEnded_ReturnsBadRequest()
        {
            var session = CreateSession("Ended");
            _context.Sessions.Add(session);
            _context.SaveChanges();

            var measurement = CreateMeasurement(session.Id);

            var result = _controller.Add(measurement);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Delete_WhenNotExists_ReturnsNotFound()
        {
            var result = _controller.Delete(999);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void Delete_WhenMeasurementExists_ReturnsOk()
        {
            var measurement = _repo.Add(CreateMeasurement(1));

            var result = _controller.Delete(measurement.Id);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public void Update_Always_ReturnsBadRequest()
        {
            var measurement = CreateMeasurement(1);

            var result = _controller.Update(1, measurement);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void GetSessionSummary_WhenSessionNotFound_ReturnsNotFound()
        {
            var result = _controller.GetSessionSummary(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void GetSessionSummary_WhenNoMeasurements_ReturnsZeroValues()
        {
            var session = CreateSession();
            _context.Sessions.Add(session);
            _context.SaveChanges();

            var result = _controller.GetSessionSummary(session.Id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }
}