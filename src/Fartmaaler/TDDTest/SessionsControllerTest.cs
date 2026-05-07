using System;
using System.Collections.Generic;
using System.Linq;
using FartmaalerAPI.Controllers;
using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using FartmaalerAPI.Services;
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
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        private Group CreateGroup()
        {
            return new Group
            {
                Name = "Test group",
                School = "Test school",
                IsLocked = false
            };
        }

        private Session CreateSession(int groupId, string status = "Started")
        {
            return new Session
            {
                GroupId = groupId,
                CarType = "Toy car",
                RoadType = "Byzone 50",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = status,
                CreatedAt = DateTime.Now
            };
        }

        [Fact]
        public void GetAll_WhenNoSessionsExist_ReturnsOkWithEmptyList()
        {
            var result = _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var sessions = Assert.IsAssignableFrom<IEnumerable<Session>>(okResult.Value);

            Assert.Empty(sessions);
        }

        [Fact]
        public void GetAll_WhenSessionsExist_ReturnsOkWithAllSessions()
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

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var sessions = Assert.IsAssignableFrom<IEnumerable<Session>>(okResult.Value);

            Assert.Equal(3, sessions.Count());
        }

        [Fact]
        public void GetById_WhenSessionExists_ReturnsOkWithSession()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var session = CreateSession(group.Id);
            _context.Sessions.Add(session);
            _context.SaveChanges();

            var result = _controller.GetById(session.Id);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSession = Assert.IsType<Session>(okResult.Value);

            Assert.Equal(session.Id, returnedSession.Id);
        }

        [Fact]
        public void GetById_WhenSessionDoesNotExist_ReturnsNotFound()
        {
            var result = _controller.GetById(999);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void Add_ValidSession_ReturnsCreated()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var session = CreateSession(group.Id);

            var result = _controller.Add(session);

            Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Single(_context.Sessions);
        }

        [Fact]
        public void Add_ValidSession_SavesCorrectProperties()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var session = CreateSession(group.Id);

            _controller.Add(session);

            var savedSession = _context.Sessions.First();

            Assert.Equal(group.Id, savedSession.GroupId);
            Assert.Equal("Toy car", savedSession.CarType);
            Assert.Equal("Byzone 50", savedSession.RoadType);
            Assert.Equal(50, savedSession.SpeedLimit);
            Assert.Equal(10, savedSession.ScalingFactor);
            Assert.Equal("Started", savedSession.Status);
        }

        [Fact]
        public void Add_WhenGroupIdInvalid_ReturnsBadRequest()
        {
            var session = CreateSession(0);

            var result = _controller.Add(session);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenGroupDoesNotExist_ReturnsNotFound()
        {
            var session = CreateSession(999);

            var result = _controller.Add(session);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenRoadTypeInvalid_ReturnsBadRequest()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var session = CreateSession(group.Id);
            session.RoadType = "Forkert vejtype";

            var result = _controller.Add(session);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenGroupIsLocked_ReturnsBadRequest()
        {
            var group = CreateGroup();
            group.IsLocked = true;

            _context.Groups.Add(group);
            _context.SaveChanges();

            var session = CreateSession(group.Id);

            var result = _controller.Add(session);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

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

        [Fact]
        public void Update_WhenSessionExists_ReturnsOk()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var session = CreateSession(group.Id);
            _context.Sessions.Add(session);
            _context.SaveChanges();

            var updatedSession = CreateSession(group.Id);
            updatedSession.CarType = "Updated car";

            var result = _controller.Update(session.Id, updatedSession);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public void Update_WhenSessionDoesNotExist_ReturnsNotFound()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var updatedSession = CreateSession(group.Id);

            var result = _controller.Update(999, updatedSession);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void EndSession_WhenSessionExists_ReturnsOkAndChangesStatus()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var session = CreateSession(group.Id);
            _context.Sessions.Add(session);
            _context.SaveChanges();

            var result = _controller.EndSession(session.Id);

            Assert.IsType<OkObjectResult>(result.Result);

            var updatedSession = _context.Sessions.First();

            Assert.Equal("Ended", updatedSession.Status);
            Assert.NotNull(updatedSession.EndedAt);
        }

        [Fact]
        public void EndSession_WhenSessionDoesNotExist_ReturnsNotFound()
        {
            var result = _controller.EndSession(999);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}