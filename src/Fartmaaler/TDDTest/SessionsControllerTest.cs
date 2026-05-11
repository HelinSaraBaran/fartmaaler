using System;
using System.Collections.Generic;
using System.Linq;
using FartmaalerAPI.Controllers;
using FartmaalerAPI.Data;
using FartmaalerAPI.DTOs;
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
            DbContextOptions<AppDbContext> options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            _repo = new SessionsRepo(_context);

            _sessionService = new SessionService(_context);

            _controller = new SessionsController(
                _repo,
                _context,
                _sessionService);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        // Opretter test gruppe
        private Group CreateGroup()
        {
            return new Group
            {
                Name = "Test group",
                School = "Test school",
                IsLocked = false
            };
        }

        // Opretter test session
        private Session CreateSession(int groupId, string status = "Started")
        {
            return new Session
            {
                GroupId = groupId,
                CarType = "Toy car",
                RoadType = "byzone 50",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = status,
                CreatedAt = DateTime.Now
            };
        }

        // Opretter request til start session
        private StartSessionRequest CreateStartSessionRequest(int groupId)
        {
            return new StartSessionRequest
            {
                GroupId = groupId,
                CarType = "Toy car",
                RoadType = "byzone 50"
            };
        }

        [Fact]
        public void GetAll_WhenNoSessionsExist_ReturnsOkWithEmptyList()
        {
            ActionResult<IEnumerable<Session>> result =
                _controller.GetAll();

            OkObjectResult okResult =
                Assert.IsType<OkObjectResult>(result.Result);

            IEnumerable<Session> sessions =
                Assert.IsAssignableFrom<IEnumerable<Session>>(okResult.Value);

            Assert.Empty(sessions);
        }

        [Fact]
        public void GetAll_WhenSessionsExist_ReturnsOkWithAllSessions()
        {
            Group group = CreateGroup();

            _context.Groups.Add(group);
            _context.SaveChanges();

            _context.Sessions.AddRange(
                CreateSession(group.Id),
                CreateSession(group.Id),
                CreateSession(group.Id)
            );

            _context.SaveChanges();

            ActionResult<IEnumerable<Session>> result =
                _controller.GetAll();

            OkObjectResult okResult =
                Assert.IsType<OkObjectResult>(result.Result);

            IEnumerable<Session> sessions =
                Assert.IsAssignableFrom<IEnumerable<Session>>(okResult.Value);

            Assert.Equal(3, sessions.Count());
        }

        [Fact]
        public void GetById_WhenSessionExists_ReturnsOkWithSession()
        {
            Group group = CreateGroup();

            _context.Groups.Add(group);
            _context.SaveChanges();

            Session session = CreateSession(group.Id);

            _context.Sessions.Add(session);
            _context.SaveChanges();

            ActionResult<Session> result =
                _controller.GetById(session.Id);

            OkObjectResult okResult =
                Assert.IsType<OkObjectResult>(result.Result);

            Session returnedSession =
                Assert.IsType<Session>(okResult.Value);

            Assert.Equal(session.Id, returnedSession.Id);
        }

        [Fact]
        public void GetById_WhenSessionDoesNotExist_ReturnsNotFound()
        {
            ActionResult<Session> result =
                _controller.GetById(999);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void Add_ValidSession_ReturnsCreated()
        {
            Group group = CreateGroup();

            _context.Groups.Add(group);
            _context.SaveChanges();

            StartSessionRequest request =
                CreateStartSessionRequest(group.Id);

            ActionResult<Session> result =
                _controller.Add(request);

            Assert.IsType<CreatedAtActionResult>(result.Result);

            Assert.Single(_context.Sessions);
        }

        [Fact]
        public void Add_ValidSession_SavesCorrectProperties()
        {
            Group group = CreateGroup();

            _context.Groups.Add(group);
            _context.SaveChanges();

            StartSessionRequest request =
                CreateStartSessionRequest(group.Id);

            _controller.Add(request);

            Session savedSession =
                _context.Sessions.First();

            Assert.Equal(group.Id, savedSession.GroupId);

            Assert.Equal("Toy car", savedSession.CarType);

            Assert.Equal("byzone 50", savedSession.RoadType);

            Assert.Equal(50, savedSession.SpeedLimit);

            Assert.Equal(10, savedSession.ScalingFactor);

            Assert.Equal("Started", savedSession.Status);
        }

        [Fact]
        public void Add_WhenGroupIdInvalid_ReturnsBadRequest()
        {
            StartSessionRequest request =
                CreateStartSessionRequest(0);

            ActionResult<Session> result =
                _controller.Add(request);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenGroupDoesNotExist_ReturnsNotFound()
        {
            StartSessionRequest request =
                CreateStartSessionRequest(999);

            ActionResult<Session> result =
                _controller.Add(request);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenRoadTypeInvalid_ReturnsBadRequest()
        {
            Group group = CreateGroup();

            _context.Groups.Add(group);
            _context.SaveChanges();

            StartSessionRequest request = new StartSessionRequest
            {
                GroupId = group.Id,
                CarType = "Toy car",
                RoadType = "Forkert vejtype"
            };

            ActionResult<Session> result =
                _controller.Add(request);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Add_WhenGroupIsLocked_ReturnsBadRequest()
        {
            Group group = CreateGroup();

            group.IsLocked = true;

            _context.Groups.Add(group);
            _context.SaveChanges();

            StartSessionRequest request =
                CreateStartSessionRequest(group.Id);

            ActionResult<Session> result =
                _controller.Add(request);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Delete_WhenSessionExists_ReturnsOk()
        {
            Group group = CreateGroup();

            _context.Groups.Add(group);
            _context.SaveChanges();

            Session session = CreateSession(group.Id);

            _context.Sessions.Add(session);
            _context.SaveChanges();

            ActionResult<Session> result =
                _controller.Delete(session.Id);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public void Delete_WhenSessionDoesNotExist_ReturnsNotFound()
        {
            ActionResult<Session> result =
                _controller.Delete(999);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void Update_WhenSessionExists_ReturnsOk()
        {
            Group group = CreateGroup();

            _context.Groups.Add(group);
            _context.SaveChanges();

            Session session = CreateSession(group.Id);

            _context.Sessions.Add(session);
            _context.SaveChanges();

            Session updatedSession =
                CreateSession(group.Id);

            updatedSession.CarType = "Updated car";

            ActionResult<Session> result =
                _controller.Update(session.Id, updatedSession);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public void Update_WhenSessionDoesNotExist_ReturnsNotFound()
        {
            Group group = CreateGroup();

            _context.Groups.Add(group);
            _context.SaveChanges();

            Session updatedSession =
                CreateSession(group.Id);

            ActionResult<Session> result =
                _controller.Update(999, updatedSession);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void EndSession_WhenSessionExists_ReturnsOkAndChangesStatus()
        {
            Group group = CreateGroup();

            _context.Groups.Add(group);
            _context.SaveChanges();

            Session session = CreateSession(group.Id);

            _context.Sessions.Add(session);
            _context.SaveChanges();

            ActionResult<Session> result =
                _controller.EndSession(session.Id);

            Assert.IsType<OkObjectResult>(result.Result);

            Session updatedSession =
                _context.Sessions.First();

            Assert.Equal("Ended", updatedSession.Status);

            Assert.NotNull(updatedSession.EndedAt);
        }

        [Fact]
        public void EndSession_WhenSessionDoesNotExist_ReturnsNotFound()
        {
            ActionResult<Session> result =
                _controller.EndSession(999);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}