using System;
using FartmaalerAPI.Controllers;
using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TDDTest
{
    // Tester SessionsController, så vi ved at endpoints og session logik virker korrekt
    // Testene bruger en InMemory database, så vi ikke rammer den rigtige database
    public class SessionsControllerTest : IDisposable
    {
        // Test database context
        private readonly AppDbContext _context;

        // Repository som controlleren bruger
        private readonly SessionsRepo _repo;

        // Controller som vi tester
        private readonly SessionsController _controller;

        // Constructor kører før hver test
        public SessionsControllerTest()
        {
            // Opretter en unik InMemory database til hver test
            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Opretter context
            _context = new AppDbContext(options);

            // Opretter repository
            _repo = new SessionsRepo(_context);

            // Opretter controller
            _controller = new SessionsController(_repo, _context);
        }

        // Dispose rydder op efter testen
        public void Dispose()
        {
            _context.Dispose();
        }

        // Hjælpemetode der opretter en standard test gruppe
        private Group CreateGroup()
        {
            return new Group
            {
                Name = "Test Group",
                School = "Test School",
                IsLocked = false
            };
        }

        // Hjælpemetode der opretter en standard test session
        private Session CreateSession(int groupId, string roadType = "Byzone 50")
        {
            return new Session
            {
                GroupId = groupId,
                CarType = "Toy car",
                RoadType = roadType,
                SpeedLimit = 0,
                ScalingFactor = 0,
                Status = "",
                CreatedAt = DateTime.Now,
                EndedAt = null
            };
        }

        #region ADD TESTS

        [Fact]
        public void Add_ValidSession_ReturnsCreatedAtAction()
        {
            // Arrange - opretter en gruppe
            Group group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            // Arrange - opretter en session til gruppen
            Session session = CreateSession(group.Id);

            // Act - opretter session via controller
            ActionResult<Session> result = _controller.Add(session);

            // Assert - tjekker at vi får CreatedAtAction
            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        [Fact]
        public void Add_ValidSession_AddsSessionToDatabase()
        {
            // Arrange - opretter en gruppe
            Group group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            // Arrange - opretter en session til gruppen
            Session session = CreateSession(group.Id);

            // Act - opretter session via controller
            _controller.Add(session);

            // Assert - tjekker at sessionen blev gemt
            Assert.Single(_context.Sessions);
        }

        [Fact]
        public void Add_ValidSession_LocksGroup()
        {
            // Arrange - opretter en gruppe
            Group group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            // Arrange - opretter en session til gruppen
            Session session = CreateSession(group.Id);

            // Act - opretter session via controller
            _controller.Add(session);

            // Assert - tjekker at gruppen bliver låst
            Assert.True(group.IsLocked);
        }

        [Fact]
        public void Add_Byzone50_SetsSpeedLimitAndScalingFactor()
        {
            // Arrange - opretter en gruppe
            Group group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            // Arrange - opretter session med Byzone 50
            Session session = CreateSession(group.Id, "Byzone 50");

            // Act - opretter session via controller
            ActionResult<Session> result = _controller.Add(session);

            // Assert - henter det oprettede objekt
            CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Session createdSession = Assert.IsType<Session>(createdResult.Value);

            // Assert - tjekker værdier
            Assert.Equal(50, createdSession.SpeedLimit);
            Assert.Equal(10, createdSession.ScalingFactor);
            Assert.Equal("Started", createdSession.Status);
        }

        [Fact]
        public void Add_Landevej80_SetsSpeedLimitAndScalingFactor()
        {
            // Arrange - opretter en gruppe
            Group group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            // Arrange - opretter session med Landevej 80
            Session session = CreateSession(group.Id, "Landevej 80");

            // Act - opretter session via controller
            ActionResult<Session> result = _controller.Add(session);

            // Assert - henter det oprettede objekt
            CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Session createdSession = Assert.IsType<Session>(createdResult.Value);

            // Assert - tjekker værdier
            Assert.Equal(80, createdSession.SpeedLimit);
            Assert.Equal(15, createdSession.ScalingFactor);
        }

        [Fact]
        public void Add_Motorvej130_SetsSpeedLimitAndScalingFactor()
        {
            // Arrange - opretter en gruppe
            Group group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            // Arrange - opretter session med Motorvej 130
            Session session = CreateSession(group.Id, "Motorvej 130");

            // Act - opretter session via controller
            ActionResult<Session> result = _controller.Add(session);

            // Assert - henter det oprettede objekt
            CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Session createdSession = Assert.IsType<Session>(createdResult.Value);

            // Assert - tjekker værdier
            Assert.Equal(130, createdSession.SpeedLimit);
            Assert.Equal(20, createdSession.ScalingFactor);
        }

        [Fact]
        public void Add_WhenGroupIdIsInvalid_ReturnsBadRequest()
        {
            // Arrange - opretter session uden gyldigt GroupId
            Session session = CreateSession(0);

            // Act - forsøger at oprette session
            ActionResult<Session> result = _controller.Add(session);

            // Assert - tjekker at vi får BadRequest
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenCarTypeIsEmpty_ReturnsBadRequest()
        {
            // Arrange - opretter en session uden biltype
            Session session = CreateSession(1);
            session.CarType = "";

            // Act - forsøger at oprette session
            ActionResult<Session> result = _controller.Add(session);

            // Assert - tjekker at vi får BadRequest
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenRoadTypeIsEmpty_ReturnsBadRequest()
        {
            // Arrange - opretter en session uden vejtype
            Session session = CreateSession(1);
            session.RoadType = "";

            // Act - forsøger at oprette session
            ActionResult<Session> result = _controller.Add(session);

            // Assert - tjekker at vi får BadRequest
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenGroupDoesNotExist_ReturnsNotFound()
        {
            // Arrange - opretter session med GroupId der ikke findes
            Session session = CreateSession(999);

            // Act - forsøger at oprette session
            ActionResult<Session> result = _controller.Add(session);

            // Assert - tjekker at vi får NotFound
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenGroupIsLocked_ReturnsBadRequest()
        {
            // Arrange - opretter en låst gruppe
            Group group = CreateGroup();
            group.IsLocked = true;

            _context.Groups.Add(group);
            _context.SaveChanges();

            // Arrange - opretter en session til den låste gruppe
            Session session = CreateSession(group.Id);

            // Act - forsøger at oprette session
            ActionResult<Session> result = _controller.Add(session);

            // Assert - tjekker at vi får BadRequest
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenRoadTypeIsInvalid_ReturnsBadRequest()
        {
            // Arrange - opretter en gruppe
            Group group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            // Arrange - opretter session med ugyldig vejtype
            Session session = CreateSession(group.Id, "Forkert vejtype");

            // Act - forsøger at oprette session
            ActionResult<Session> result = _controller.Add(session);

            // Assert - tjekker at vi får BadRequest
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        #endregion

        #region GET TESTS

        [Fact]
        public void GetById_WhenNotExists_ReturnsNotFound()
        {
            // Act - forsøger at hente en session der ikke findes
            ActionResult<Session> result = _controller.GetById(999);

            // Assert - tjekker at vi får NotFound
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        #endregion

        #region DELETE TESTS

        [Fact]
        public void Delete_WhenNotExists_ReturnsNotFound()
        {
            // Act - forsøger at slette en session der ikke findes
            ActionResult<Session> result = _controller.Delete(999);

            // Assert - tjekker at vi får NotFound
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        #endregion
    }
}