using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using FartmaalerAPI.Controllers;
using FartmaalerAPI.Data;
using FartmaalerAPI.DTOs;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using FartmaalerAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TDDTest
{
    public class SessionsControllerTest : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly SessionsRepo _repo;
        private readonly SessionService _sessionService;
        private readonly SessionsController _controller;

        public SessionsControllerTest()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repo = new SessionsRepo(_context);
            _sessionService = new SessionService(_context);
            _controller = new SessionsController(_repo, _context, _sessionService);

            // Admin-rolle til Delete og Update
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "admin")
            }, "TestAuth"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        public void Dispose() => _context.Dispose();

        // ── Hjælpemetoder ──────────────────────────────────────────

        private Group CreateGroup(bool isLocked = false) => new Group
        {
            Name = "Test group",
            School = "Test school",
            IsLocked = isLocked
        };

        private StartSessionRequest CreateRequest(
            int groupId,
            string roadType = "Byzone 50",
            string carType = "Toy car") =>
            new StartSessionRequest
            {
                GroupId = groupId,
                CarType = carType,
                RoadType = roadType
            };

        private Session CreateSession(int groupId, string status = "Started") => new Session
        {
            GroupId = groupId,
            CarType = "Toy car",
            RoadType = "byzone 50",
            SpeedLimit = 50,
            ScalingFactor = 10,
            Status = status,
            CreatedAt = DateTime.Now
        };

        // ── GetAll ─────────────────────────────────────────────────

        [Fact]
        public void GetAll_WhenNoSessionsExist_ReturnsOkWithEmptyList()
        {
            var result = _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var sessions = Assert.IsAssignableFrom<IEnumerable<Session>>(ok.Value);
            Assert.Empty(sessions);
        }

        [Fact]
        public void GetAll_WhenSessionsExist_ReturnsAllSessions()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            _context.Sessions.AddRange(
                CreateSession(group.Id),
                CreateSession(group.Id),
                CreateSession(group.Id)
            );
            _context.SaveChanges();

            var result = _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var sessions = Assert.IsAssignableFrom<IEnumerable<Session>>(ok.Value);
            Assert.Equal(3, sessions.Count());
        }

        // ── GetById ────────────────────────────────────────────────

        [Fact]
        public void GetById_WhenSessionExists_ReturnsOk()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var session = CreateSession(group.Id);
            _context.Sessions.Add(session);
            _context.SaveChanges();

            var result = _controller.GetById(session.Id);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<Session>(ok.Value);
            Assert.Equal(session.Id, returned.Id);
        }

        [Fact]
        public void GetById_WhenSessionDoesNotExist_ReturnsNotFound()
        {
            var result = _controller.GetById(999);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        // ── Add ────────────────────────────────────────────────────

        [Fact]
        public void Add_ValidRequest_ReturnsCreated()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var result = _controller.Add(CreateRequest(group.Id));

            Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Single(_context.Sessions);
        }

        [Fact]
        public void Add_ValidRequest_SavesCorrectProperties()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            _controller.Add(CreateRequest(group.Id, "Landevej 80", "Bus"));

            var saved = _context.Sessions.First();
            Assert.Equal(group.Id, saved.GroupId);
            Assert.Equal("Bus", saved.CarType);
            Assert.Equal("Started", saved.Status);
        }

        [Fact]
        public void Add_ValidRequest_LocksGroup()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            _controller.Add(CreateRequest(group.Id));

            var updatedGroup = _context.Groups.First();
            Assert.True(updatedGroup.IsLocked);
        }

        [Fact]
        public void Add_WhenGroupIdIsZero_ReturnsBadRequest()
        {
            var result = _controller.Add(CreateRequest(0));
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenCarTypeIsEmpty_ReturnsBadRequest()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var result = _controller.Add(CreateRequest(group.Id, carType: ""));
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenRoadTypeIsEmpty_ReturnsBadRequest()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var result = _controller.Add(CreateRequest(group.Id, roadType: ""));
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Theory]
        [InlineData("Byzone 50")]
        [InlineData("Landevej 80")]
        [InlineData("Motorvej 130")]
        public void Add_WithValidRoadTypes_ReturnsCreated(string roadType)
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var result = _controller.Add(CreateRequest(group.Id, roadType));
            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        [Fact]
        public void Add_WhenRoadTypeIsInvalid_ReturnsBadRequest()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var result = _controller.Add(CreateRequest(group.Id, roadType: "Forkert vej"));
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenGroupDoesNotExist_ReturnsNotFound()
        {
            var result = _controller.Add(CreateRequest(999));
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenGroupIsLocked_ReturnsBadRequest()
        {
            var group = CreateGroup(isLocked: true);
            _context.Groups.Add(group);
            _context.SaveChanges();

            var result = _controller.Add(CreateRequest(group.Id));
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // ── EndSession ─────────────────────────────────────────────

        [Fact]
        public void EndSession_WhenSessionExists_ReturnsOk()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var session = CreateSession(group.Id);
            _context.Sessions.Add(session);
            _context.SaveChanges();

            var result = _controller.EndSession(session.Id);
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public void EndSession_WhenSessionExists_SetsStatusToEnded()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var session = CreateSession(group.Id);
            _context.Sessions.Add(session);
            _context.SaveChanges();

            _controller.EndSession(session.Id);

            var updated = _context.Sessions.First();
            Assert.Equal("Ended", updated.Status);
            Assert.NotNull(updated.EndedAt);
        }

        [Fact]
        public void EndSession_WhenSessionDoesNotExist_ReturnsNotFound()
        {
            var result = _controller.EndSession(999);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        // ── Update ─────────────────────────────────────────────────

        [Fact]
        public void Update_WhenSessionExists_ReturnsOk()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var session = CreateSession(group.Id);
            _context.Sessions.Add(session);
            _context.SaveChanges();

            var updated = CreateSession(group.Id);
            updated.CarType = "Updated car";

            var result = _controller.Update(session.Id, updated);
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public void Update_WhenSessionDoesNotExist_ReturnsNotFound()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var result = _controller.Update(999, CreateSession(group.Id));
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        // ── Delete ─────────────────────────────────────────────────

        [Fact]
        public void Delete_WhenSessionExists_ReturnsOk()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var session = CreateSession(group.Id);
            _context.Sessions.Add(session);
            _context.SaveChanges();

            var result = _controller.Delete(session.Id);
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public void Delete_WhenSessionDoesNotExist_ReturnsNotFound()
        {
            var result = _controller.Delete(999);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}