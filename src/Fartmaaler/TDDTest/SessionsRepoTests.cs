using System;
using System.Linq;
using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TDDTest
{
    public class SessionsRepoTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly SessionsRepo _repo;

        public SessionsRepoTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repo = new SessionsRepo(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        private Session CreateSession()
        {
            return new Session
            {
                GroupId = 1,
                CarType = "Toy car",
                RoadType = "Asphalt",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = "Started",
                CreatedAt = DateTime.Now,
                EndedAt = null
            };
        }

        #region GET TESTS

        [Fact]
        public void GetAll_WhenRepoIsEmpty_ReturnsEmptyList()
        {
            var result = _repo.GetAll();

            Assert.Empty(result);
        }

        [Fact]
        public void GetAll_WhenSessionsExist_ReturnsAllSessions()
        {
            _context.Sessions.AddRange(
                CreateSession(),
                CreateSession(),
                CreateSession()
            );
            _context.SaveChanges();

            var result = _repo.GetAll().ToList();

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetById_WhenIdExists_ReturnsSession()
        {
            var session = CreateSession();
            _context.Sessions.Add(session);
            _context.SaveChanges();

            var result = _repo.GetById(session.Id);

            Assert.NotNull(result);
            Assert.Equal(session.Id, result.Id);
        }

        [Fact]
        public void GetById_WhenIdDoesNotExist_ReturnsNull()
        {
            var result = _repo.GetById(999);

            Assert.Null(result);
        }

        #endregion

        #region ADD TESTS

        [Fact]
        public void Add_ValidSession_AddsSessionToDatabase()
        {
            var session = CreateSession();

            var result = _repo.Add(session);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Single(_context.Sessions);
        }

        [Fact]
        public void Add_ValidSession_PropertiesAreSavedCorrectly()
        {
            var session = CreateSession();

            var result = _repo.Add(session);

            Assert.Equal(1, result.GroupId);
            Assert.Equal("Toy car", result.CarType);
            Assert.Equal("Asphalt", result.RoadType);
            Assert.Equal(50, result.SpeedLimit);
            Assert.Equal(10, result.ScalingFactor);
            Assert.Equal("Started", result.Status);
            Assert.Null(result.EndedAt);
        }

        [Fact]
        public void Add_ValidSession_CanBeFoundById()
        {
            var session = CreateSession();

            var added = _repo.Add(session);
            var found = _repo.GetById(added.Id);

            Assert.NotNull(found);
            Assert.Equal(added.Id, found.Id);
        }

        #endregion

        #region DELETE TESTS

        [Fact]
        public void Delete_WhenIdDoesNotExist_ReturnsNull()
        {
            var result = _repo.Delete(999);

            Assert.Null(result);
        }

        [Fact]
        public void Delete_WhenIdExists_RemovesSessionFromDatabase()
        {
            var session = _repo.Add(CreateSession());

            var deleted = _repo.Delete(session.Id);
            var all = _repo.GetAll();

            Assert.NotNull(deleted);
            Assert.Empty(all);
        }

        [Fact]
        public void Delete_WhenIdExists_ReturnsDeletedSession()
        {
            var session = _repo.Add(CreateSession());

            var result = _repo.Delete(session.Id);

            Assert.NotNull(result);
            Assert.Equal(session.Id, result.Id);
            Assert.Equal("Toy car", result.CarType);
        }

        [Fact]
        public void Delete_OneOfMultiple_RemovesOnlySelectedSession()
        {
            var s1 = _repo.Add(CreateSession());
            var s2 = _repo.Add(CreateSession());

            _repo.Delete(s1.Id);
            var all = _repo.GetAll().ToList();

            Assert.Single(all);
            Assert.Equal(s2.Id, all[0].Id);
        }

        #endregion

        #region UPDATE TESTS

        [Fact]
        public void Update_WhenIdDoesNotExist_ReturnsNull()
        {
            var updatedSession = CreateSession();

            var result = _repo.Update(999, updatedSession);

            Assert.Null(result);
        }

        [Fact]
        public void Update_WhenIdExists_UpdatesSession()
        {
            var session = _repo.Add(CreateSession());

            var updatedSession = new Session
            {
                GroupId = 2,
                CarType = "Ball",
                RoadType = "Wood",
                SpeedLimit = 80,
                ScalingFactor = 5,
                Status = "Ended",
                EndedAt = DateTime.Now
            };

            var result = _repo.Update(session.Id, updatedSession);

            Assert.NotNull(result);
            Assert.Equal(2, result.GroupId);
            Assert.Equal("Ball", result.CarType);
            Assert.Equal("Wood", result.RoadType);
            Assert.Equal(80, result.SpeedLimit);
            Assert.Equal(5, result.ScalingFactor);
            Assert.Equal("Ended", result.Status);
            Assert.NotNull(result.EndedAt);
        }

        [Fact]
        public void Update_WhenIdExists_DoesNotChangeId()
        {
            var session = _repo.Add(CreateSession());

            var updatedSession = CreateSession();
            updatedSession.Id = 999;

            var result = _repo.Update(session.Id, updatedSession);

            Assert.NotNull(result);
            Assert.Equal(session.Id, result.Id);
            Assert.NotEqual(999, result.Id);
        }

        [Fact]
        public void Update_OneOfMultiple_UpdatesOnlySelectedSession()
        {
            var s1 = _repo.Add(CreateSession());
            var s2 = _repo.Add(CreateSession());

            var updatedSession = new Session
            {
                GroupId = 99,
                CarType = "Updated",
                RoadType = "UpdatedRoad",
                SpeedLimit = 130,
                ScalingFactor = 20,
                Status = "Ended",
                EndedAt = DateTime.Now
            };

            _repo.Update(s1.Id, updatedSession);

            var firstResult = _repo.GetById(s1.Id);
            var secondResult = _repo.GetById(s2.Id);

            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);

            Assert.Equal("Updated", firstResult.CarType);
            Assert.Equal("Toy car", secondResult.CarType);
        }

        #endregion
    }
}