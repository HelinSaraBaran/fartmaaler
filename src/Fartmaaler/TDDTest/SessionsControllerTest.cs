using FartmaalerAPI.Controllers;
using FartmaalerAPI.Data;
using FartmaalerAPI.DTOs;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories.Interfaces;
using FartmaalerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FartmaalerAPI.Tests
{
    public class SessionsControllerTests
    {
        // Opretter fake database til tests
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        // Opretter controller med fake dependencies
        private SessionsController CreateController(AppDbContext context)
        {
            var repoMock = new Mock<IRepository<Session>>();

            var sessionService = new SessionService(context);

            return new SessionsController(
                repoMock.Object,
                context,
                sessionService);
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
        // Tester at GetById returnerer NotFound hvis session ikke findes
        public void GetById_ReturnsNotFound_WhenSessionDoesNotExist()
        {
            using var context = GetDbContext();

            var repoMock = new Mock<IRepository<Session>>();

            repoMock.Setup(repo => repo.GetById(1))
                .Returns((Session?)null);

            var controller = new SessionsController(
                repoMock.Object,
                context,
                new SessionService(context));

            var result = controller.GetById(1);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        // Tester at Add returnerer BadRequest hvis GroupId er ugyldigt
        public void Add_ReturnsBadRequest_WhenGroupIdIsInvalid()
        {
            using var context = GetDbContext();

            var controller = CreateController(context);

            var request = new StartSessionRequest
            {
                GroupId = 0,
                CarType = "diesel",
                RoadType = "byzone 50"
            };

            var result = controller.Add(request);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        // Tester at Add returnerer BadRequest hvis biltype mangler
        public void Add_ReturnsBadRequest_WhenCarTypeIsMissing()
        {
            using var context = GetDbContext();

            var controller = CreateController(context);

            var request = new StartSessionRequest
            {
                GroupId = 1,
                CarType = "",
                RoadType = "byzone 50"
            };

            var result = controller.Add(request);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        // Tester at Add returnerer BadRequest hvis vejtype er ugyldig
        public void Add_ReturnsBadRequest_WhenRoadTypeIsInvalid()
        {
            using var context = GetDbContext();

            var controller = CreateController(context);

            var request = new StartSessionRequest
            {
                GroupId = 1,
                CarType = "diesel",
                RoadType = "forkert vej"
            };

            var result = controller.Add(request);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        // Tester at Add returnerer NotFound hvis gruppen ikke findes
        public void Add_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            using var context = GetDbContext();

            var controller = CreateController(context);

            var request = new StartSessionRequest
            {
                GroupId = 1,
                CarType = "diesel",
                RoadType = "byzone 50"
            };

            var result = controller.Add(request);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        // Tester at Add opretter session korrekt
        public void Add_CreatesSession()
        {
            using var context = GetDbContext();

            context.Groups.Add(new Group
            {
                Id = 1,
                Name = "Gruppe 1",
                School = "Roskilde Skole",
                IsLocked = false
            });

            context.SaveChanges();

            var controller = CreateController(context);

            var request = new StartSessionRequest
            {
                GroupId = 1,
                CarType = "diesel",
                RoadType = "byzone 50"
            };

            var result = controller.Add(request);

            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        [Fact]
        // Tester at EndSession returnerer NotFound hvis session ikke findes
        public void EndSession_ReturnsNotFound_WhenSessionDoesNotExist()
        {
            using var context = GetDbContext();

            var controller = CreateController(context);

            var result = controller.EndSession(1);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        // Tester at EndSession afslutter session korrekt
        public void EndSession_ReturnsOk_WhenSessionExists()
        {
            using var context = GetDbContext();

            context.Groups.Add(new Group
            {
                Id = 1,
                Name = "Gruppe 1",
                School = "Roskilde Skole",
                IsLocked = true
            });

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

            var result = controller.EndSession(1);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        // Tester at Delete returnerer NotFound hvis session ikke findes
        public void Delete_ReturnsNotFound_WhenSessionDoesNotExist()
        {
            using var context = GetDbContext();

            var controller = CreateController(context);

            var result = controller.Delete(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        // Tester at Delete sletter session korrekt
        public void Delete_RemovesSession()
        {
            using var context = GetDbContext();

            context.Groups.Add(new Group
            {
                Id = 1,
                Name = "Gruppe 1",
                School = "Roskilde Skole",
                IsLocked = true
            });

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

            var result = controller.Delete(1);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        // Tester at DeleteAll returnerer Ok
        public void DeleteAll_ReturnsOk()
        {
            using var context = GetDbContext();

            var controller = CreateController(context);

            var result = controller.DeleteAll();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        // Tester at GetClassSummary returnerer Ok
        public void GetClassSummary_ReturnsOk()
        {
            using var context = GetDbContext();

            var controller = CreateController(context);

            var result = controller.GetClassSummary();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        // Tester at GetLatestActiveSession returnerer NotFound hvis ingen aktive sessions findes
        public void GetLatestActiveSession_ReturnsNotFound_WhenNoActiveSessionExists()
        {
            using var context = GetDbContext();

            var controller = CreateController(context);

            var result = controller.GetLatestActiveSession();

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        // Tester at GetLatestActiveSession returnerer Ok hvis aktiv session findes
        public void GetLatestActiveSession_ReturnsOk_WhenActiveSessionExists()
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

            var result = controller.GetLatestActiveSession();

            Assert.IsType<OkObjectResult>(result);
        }
    }
}