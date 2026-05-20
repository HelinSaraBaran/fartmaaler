using FartmaalerAPI.Controllers;
using FartmaalerAPI.Data;
using FartmaalerAPI.DTOs;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using FartmaalerAPI.Repositories.Interfaces;
using FartmaalerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FartmaalerAPI.Tests
{
    public class MeasurementsControllerTests
    {
        // Opretter fake in-memory database
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        // Opretter controller med fake dependencies
        private MeasurementsController CreateController(AppDbContext context)
        {
            var repoMock = new Mock<IRepository<Measurement>>();

            var measurementsRepo = new MeasurementsRepo(context);

            var measurementService =
                new MeasurementService(context, measurementsRepo);

            return new MeasurementsController(
                repoMock.Object,
                context,
                measurementService);
        }

        [Fact]
        // Tester at GetAll returnerer Ok
        public void GetAll_ReturnsOk()
        {
            using var context = GetDbContext();

            var controller = CreateController(context);

            var result = controller.GetAll();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        // Tester at GetById returnerer NotFound hvis målingen ikke findes
        public void GetById_ReturnsNotFound_WhenMeasurementDoesNotExist()
        {
            using var context = GetDbContext();

            var repoMock = new Mock<IRepository<Measurement>>();

            repoMock.Setup(repo => repo.GetById(1))
                .Returns((Measurement?)null);

            var controller = new MeasurementsController(
                repoMock.Object,
                context,
                new MeasurementService(
                    context,
                    new MeasurementsRepo(context)));

            var result = controller.GetById(1);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        // Tester at Add returnerer BadRequest hvis SessionId er ugyldigt
        public void Add_ReturnsBadRequest_WhenSessionIdIsInvalid()
        {
            using var context = GetDbContext();

            var controller = CreateController(context);

            var request = new CreateMeasurementRequest
            {
                SessionId = 0,
                Time = 2
            };

            var result = controller.Add(request);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        // Tester at Add returnerer BadRequest hvis tid er ugyldig
        public void Add_ReturnsBadRequest_WhenTimeIsInvalid()
        {
            using var context = GetDbContext();

            var controller = CreateController(context);

            var request = new CreateMeasurementRequest
            {
                SessionId = 1,
                Time = 0
            };

            var result = controller.Add(request);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        // Tester at Add returnerer NotFound hvis session ikke findes
        public void Add_ReturnsNotFound_WhenSessionDoesNotExist()
        {
            using var context = GetDbContext();

            var controller = CreateController(context);

            var request = new CreateMeasurementRequest
            {
                SessionId = 1,
                Time = 2
            };

            var result = controller.Add(request);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        // Tester at Add returnerer BadRequest hvis session er afsluttet
        public void Add_ReturnsBadRequest_WhenSessionIsEnded()
        {
            using var context = GetDbContext();

            context.Sessions.Add(new Session
            {
                Id = 1,
                GroupId = 1,
                CarType = "diesel",
                RoadType = "byzone 50",
                SpeedLimit = 50,
                ScalingFactor = 20,
                Status = "Ended",
                CreatedAt = DateTime.Now
            });

            context.SaveChanges();

            var controller = CreateController(context);

            var request = new CreateMeasurementRequest
            {
                SessionId = 1,
                Time = 2
            };

            var result = controller.Add(request);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        // Tester at Add opretter måling korrekt
        public void Add_CreatesMeasurement()
        {
            using var context = GetDbContext();

            context.Sessions.Add(new Session
            {
                Id = 1,
                GroupId = 1,
                CarType = "diesel",
                RoadType = "byzone 50",
                SpeedLimit = 50,
                ScalingFactor = 20,
                Status = "Active",
                CreatedAt = DateTime.Now
            });

            context.SaveChanges();

            var controller = CreateController(context);

            var request = new CreateMeasurementRequest
            {
                SessionId = 1,
                Time = 2
            };

            var result = controller.Add(request);

            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        [Fact]
        // Tester at Delete returnerer NotFound hvis målingen ikke findes
        public void Delete_ReturnsNotFound_WhenMeasurementDoesNotExist()
        {
            using var context = GetDbContext();

            var repoMock = new Mock<IRepository<Measurement>>();

            repoMock.Setup(repo => repo.Delete(1))
                .Returns((Measurement?)null);

            var controller = new MeasurementsController(
                repoMock.Object,
                context,
                new MeasurementService(
                    context,
                    new MeasurementsRepo(context)));

            var result = controller.Delete(1);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        // Tester at Update altid returnerer BadRequest
        public void Update_ReturnsBadRequest()
        {
            using var context = GetDbContext();

            var controller = CreateController(context);

            var measurement = new Measurement();

            var result = controller.Update(1, measurement);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        // Tester at GetLeaderboard returnerer NotFound hvis ingen data findes
        public void GetLeaderboard_ReturnsNotFound_WhenNoDataExists()
        {
            using var context = GetDbContext();

            var controller = CreateController(context);

            var result = controller.GetLeaderboard();

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        // Tester at GetMeasurementsBySession returnerer NotFound hvis session ikke findes
        public void GetMeasurementsBySession_ReturnsNotFound_WhenSessionDoesNotExist()
        {
            using var context = GetDbContext();

            var controller = CreateController(context);

            var result = controller.GetMeasurementsBySession(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        // Tester at GetSessionSummary returnerer NotFound hvis session ikke findes
        public void GetSessionSummary_ReturnsNotFound_WhenSessionDoesNotExist()
        {
            using var context = GetDbContext();

            var controller = CreateController(context);

            var result = controller.GetSessionSummary(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        // Tester at GetLiveOverview returnerer Ok
        public void GetLiveOverview_ReturnsOk()
        {
            using var context = GetDbContext();

            var controller = CreateController(context);

            var result = controller.GetLiveOverview();

            Assert.IsType<OkObjectResult>(result);
        }
    }
}