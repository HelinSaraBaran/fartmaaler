using FartmaalerAPI.Controllers;
using FartmaalerAPI.Data;
using FartmaalerAPI.DTOs;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using FartmaalerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
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
            DbContextOptions<AppDbContext> options =
                new DbContextOptionsBuilder<AppDbContext>()
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

        // Opretter en test session
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

        // Opretter en manuel test måling
        private Measurement CreateMeasurement(int sessionId)
        {
            return new Measurement
            {
                SessionId = sessionId,
                Time = 1,
                Distance = 5,
                MeasuredSpeed = 18,
                SimulatedSpeed = 180,
                SpeedLimit = 50,
                Status = "Too fast",
                Co2 = 0,
                Co2Saved = 0,
                CreatedAt = DateTime.Now
            };
        }

        // Opretter request til måling
        private CreateMeasurementRequest CreateMeasurementRequest(int sessionId, double time)
        {
            return new CreateMeasurementRequest
            {
                SessionId = sessionId,
                Time = time
            };
        }

        [Fact]
        public void Add_ValidMeasurement_ReturnsCreated()
        {
            Session session = CreateSession();

            _context.Sessions.Add(session);
            _context.SaveChanges();

            CreateMeasurementRequest request =
                CreateMeasurementRequest(session.Id, 1);

            ActionResult<Measurement> result =
                _controller.Add(request);

            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        [Fact]
        public void Add_ValidMeasurement_SavesMeasurementInDatabase()
        {
            Session session = CreateSession();

            _context.Sessions.Add(session);
            _context.SaveChanges();

            CreateMeasurementRequest request =
                CreateMeasurementRequest(session.Id, 1);

            _controller.Add(request);

            Assert.Single(_context.Measurements);
        }

        [Fact]
        public void Add_ValidMeasurement_CalculatesCorrectValues()
        {
            Session session = CreateSession();

            _context.Sessions.Add(session);
            _context.SaveChanges();

            CreateMeasurementRequest request =
                CreateMeasurementRequest(session.Id, 1);

            _controller.Add(request);

            Measurement savedMeasurement =
                _context.Measurements.First();

            Assert.Equal(5, savedMeasurement.Distance);
            Assert.Equal(1, savedMeasurement.Time);
            Assert.Equal(18, savedMeasurement.MeasuredSpeed);
            Assert.Equal(180, savedMeasurement.SimulatedSpeed);
            Assert.Equal(50, savedMeasurement.SpeedLimit);
            Assert.Equal("Too fast", savedMeasurement.Status);
        }

        [Fact]
        public void Add_WhenSessionIdInvalid_ReturnsBadRequest()
        {
            CreateMeasurementRequest request =
                CreateMeasurementRequest(0, 1);

            ActionResult<Measurement> result =
                _controller.Add(request);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenSessionNotFound_ReturnsNotFound()
        {
            CreateMeasurementRequest request =
                CreateMeasurementRequest(999, 1);

            ActionResult<Measurement> result =
                _controller.Add(request);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenTimeIsZero_ReturnsBadRequest()
        {
            Session session = CreateSession();

            _context.Sessions.Add(session);
            _context.SaveChanges();

            CreateMeasurementRequest request =
                CreateMeasurementRequest(session.Id, 0);

            ActionResult<Measurement> result =
                _controller.Add(request);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenSessionEnded_ReturnsBadRequest()
        {
            Session session = CreateSession("Ended");

            _context.Sessions.Add(session);
            _context.SaveChanges();

            CreateMeasurementRequest request =
                CreateMeasurementRequest(session.Id, 1);

            ActionResult<Measurement> result =
                _controller.Add(request);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Delete_WhenNotExists_ReturnsNotFound()
        {
            ActionResult<Measurement> result =
                _controller.Delete(999);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void Delete_WhenMeasurementExists_ReturnsOk()
        {
            Measurement measurement =
                _repo.Add(CreateMeasurement(1));

            ActionResult<Measurement> result =
                _controller.Delete(measurement.Id);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public void Update_Always_ReturnsBadRequest()
        {
            Measurement measurement =
                CreateMeasurement(1);

            IActionResult result =
                _controller.Update(1, measurement);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void GetSessionSummary_WhenSessionNotFound_ReturnsNotFound()
        {
            IActionResult result =
                _controller.GetSessionSummary(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void GetSessionSummary_WhenNoMeasurements_ReturnsZeroValues()
        {
            Session session = CreateSession();

            _context.Sessions.Add(session);
            _context.SaveChanges();

            IActionResult result =
                _controller.GetSessionSummary(session.Id);

            OkObjectResult okResult =
                Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);
        }
    }
}