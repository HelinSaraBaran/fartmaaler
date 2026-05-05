using System;
using System.Linq;
using FartmaalerAPI.Controllers;
using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace TDDTest
{
    // Tester GroupsController, så vi ved at endpoints og delete-logik virker korrekt
    // Testene bruger en InMemory database, så vi ikke rammer den rigtige database
    public class GroupsControllerTest : IDisposable
    {
        // Test database context
        private readonly AppDbContext _context;

        // Repository som controlleren bruger
        private readonly GroupsRepo _repo;

        // Controller som vi tester
        private readonly GroupsController _controller;

        // Constructor kører før hver test
        public GroupsControllerTest()
        {
            // Opretter en unik InMemory database til hver test
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Opretter context
            _context = new AppDbContext(options);

            // Opretter repository
            _repo = new GroupsRepo(_context);

            // Opretter controller
            _controller = new GroupsController(_repo, _context);
        }

        // Dispose rydder op efter testen
        public void Dispose()
        {
            _context.Dispose();
        }

        #region GET TESTS

        [Fact]
        public void GetById_WhenNotExists_ReturnsNotFound()
        {
            // Act - forsøger at hente en gruppe der ikke findes
            var result = _controller.GetById(999);

            // Assert - tjekker at vi får NotFound
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        #endregion

        #region DELETE TESTS

        [Fact]
        public void Delete_WhenGroupHasSessionsAndMeasurements_DeletesEverything()
        {
            // Arrange - opretter en gruppe
            var group = new Group
            {
                Name = "Test Group",
                School = "Test School",
                IsLocked = false
            };

            _context.Groups.Add(group);
            _context.SaveChanges();

            // Arrange - opretter en session til gruppen
            var session = new Session
            {
                GroupId = group.Id,
                CarType = "Toy car",
                RoadType = "Asphalt",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = "Started",
                CreatedAt = DateTime.Now,
                EndedAt = null
            };

            _context.Sessions.Add(session);
            _context.SaveChanges();

            // Arrange - opretter en measurement til sessionen
            var measurement = new Measurement
            {
                SessionId = session.Id,
                MeasuredSpeed = 40,
                SimulatedSpeed = 400,
                Time = 2,
                Distance = 20,
                Co2 = 10,
                Co2Saved = 5,
                CreatedAt = DateTime.Now
            };

            _context.Measurements.Add(measurement);
            _context.SaveChanges();

            // Act - sletter gruppen via controller
            var result = _controller.Delete(group.Id);

            // Assert - tjekker at vi får OK response
            Assert.IsType<OkObjectResult>(result.Result);

            // Assert - tjekker at alle data er slettet
            Assert.Empty(_context.Groups);
            Assert.Empty(_context.Sessions);
            Assert.Empty(_context.Measurements);
        }

        #endregion
    }
}