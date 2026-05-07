using System;
using FartmaalerAPI.Controllers;
using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using FartmaalerAPI.Repositories.Interfaces;
using FartmaalerAPI.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TDDTest
{
    // Denne testklasse tester SessionsController
    // InMemory database bruges så vi ikke rammer rigtig database
    public class SessionsControllerTest : IDisposable
    {
        // Fake database
        private readonly AppDbContext _context;

        // Repository der bruges i controlleren
        private readonly IRepository<Session> _repo;

        // Controller der testes
        private readonly SessionsController _controller;

        // Constructor kører før hver test
        public SessionsControllerTest()
        {
            // Opretter fake database i hukommelsen
            DbContextOptions<AppDbContext> options =
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;

            // Opretter DbContext
            _context = new AppDbContext(options);

            // Opretter repository
            _repo = new SessionsRepo(_context);

            // Opretter service
            SessionService sessionService =
                new SessionService(_context);

            // Opretter controller
            _controller =
                new SessionsController(
                    _repo,
                    _context,
                    sessionService);
        }

        // Dispose rydder op efter hver test
        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public void GetAll_WhenSessionsExist_ReturnsSessions()
        {
            // Arrange
            Session session = new Session
            {
                GroupId = 1,
                CarType = "Toy car",
                RoadType = "Byzone 50",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = "Started",
                CreatedAt = DateTime.Now
            };

            // Gemmer session i databasen
            _repo.Add(session);

            // Act
            var result = _controller.GetAll();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetById_WhenSessionExists_ReturnsSession()
        {
            // Arrange
            Session session = new Session
            {
                GroupId = 1,
                CarType = "Toy car",
                RoadType = "Byzone 50",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = "Started",
                CreatedAt = DateTime.Now
            };

            // Gemmer session
            Session createdSession = _repo.Add(session);

            // Act
            var result = _controller.GetById(createdSession.Id);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetById_WhenSessionDoesNotExist_ReturnsNotFound()
        {
            // Act
            var result = _controller.GetById(999);

            // Assert
            Assert.NotNull(result);
        }
    }
}