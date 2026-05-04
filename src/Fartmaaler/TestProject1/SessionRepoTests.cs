using FartmaalerAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FartmaalerAPI.Models;

namespace TestProject1
{
    public class SessionsRepoTests
    {
        #region GET TESTS

        [Fact]
        public void GetAll_WhenRepoIsEmpty_ReturnsEmptyList()
        {
            // Arrange
            SessionsRepo repo = new SessionsRepo();

            // Act
            var result = repo.GetAll();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetById_WhenIdDoesNotExist_ReturnsNull()
        {
            // Arrange
            SessionsRepo repo = new SessionsRepo();

            // Act
            var result = repo.GetById(999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ADD TESTS

        [Fact]
        public void Add_ValidSession_ReturnsSessionWithId()
        {
            // Arrange
            SessionsRepo repo = new SessionsRepo();

            Session session = new Session
            {
                GroupId = 1,
                CarType = "Toy car",
                RoadType = "Asphalt",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = "Active",
                CreatedAt = DateTime.Now,
                EndedAt = null
            };

            // Act
            var result = repo.Add(session);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
        }

        [Fact]
        public void Add_ValidSession_PropertiesAreCopiedCorrectly()
        {
            // Arrange
            SessionsRepo repo = new SessionsRepo();

            Session session = new Session
            {
                GroupId = 2,
                CarType = "Truck",
                RoadType = "Gravel",
                SpeedLimit = 80,
                ScalingFactor = 20,
                Status = "Active",
                CreatedAt = DateTime.Now,
                EndedAt = DateTime.Now.AddHours(1)
            };

            // Act
            var result = repo.Add(session);

            // Assert
            Assert.Equal(session.GroupId, result.GroupId);
            Assert.Equal(session.CarType, result.CarType);
            Assert.Equal(session.RoadType, result.RoadType);
            Assert.Equal(session.SpeedLimit, result.SpeedLimit);
            Assert.Equal(session.ScalingFactor, result.ScalingFactor);
            Assert.Equal(session.Status, result.Status);
            Assert.Equal(session.CreatedAt, result.CreatedAt);
            Assert.Equal(session.EndedAt, result.EndedAt);
        }

        [Fact]
        public void Add_ValidSession_IsAddedToList()
        {
            // Arrange
            SessionsRepo repo = new SessionsRepo();

            Session session = new Session
            {
                GroupId = 1,
                CarType = "Toy car",
                RoadType = "Asphalt",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            // Act
            var added = repo.Add(session);
            var all = repo.GetAll();

            // Assert
            Assert.Single(all);
            Assert.Equal(added.Id, all.First().Id);
        }

        [Fact]
        public void Add_ReturnsCopy_NotSameReference()
        {
            // Arrange
            SessionsRepo repo = new SessionsRepo();

            Session session = new Session
            {
                GroupId = 1,
                CarType = "Toy car"
            };

            // Act
            var result = repo.Add(session);

            // Assert
            Assert.NotSame(session, result);
        }

        #endregion

        #region DELETE TESTS

        [Fact]
        public void Delete_IdDoesNotExist_ReturnsNull()
        {
            // Arrange
            SessionsRepo repo = new SessionsRepo();

            // Act
            var result = repo.Delete(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Delete_ExistingId_RemovesSession()
        {
            // Arrange
            SessionsRepo repo = new SessionsRepo();

            var session = new Session
            {
                GroupId = 1,
                CarType = "Toy car"
            };

            var added = repo.Add(session);

            // Act
            repo.Delete(added.Id);
            var all = repo.GetAll();

            // Assert
            Assert.Empty(all);
        }

        [Fact]
        public void Delete_ExistingId_ReturnsDeletedSession()
        {
            // Arrange
            SessionsRepo repo = new SessionsRepo();

            var session = new Session
            {
                GroupId = 2,
                CarType = "Truck",
                SpeedLimit = 80
            };

            var added = repo.Add(session);

            // Act
            var result = repo.Delete(added.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(added.Id, result.Id);
            Assert.Equal(2, result.GroupId);
            Assert.Equal("Truck", result.CarType);
            Assert.Equal(80, result.SpeedLimit);
        }

        [Fact]
        public void Delete_ReturnsCopy_NotSameReference()
        {
            // Arrange
            SessionsRepo repo = new SessionsRepo();

            var session = new Session { GroupId = 1 };
            var added = repo.Add(session);

            // Act
            var result = repo.Delete(added.Id);

            // Assert
            Assert.NotSame(added, result);
        }

        [Fact]
        public void Delete_OneOfMultiple_RemovesOnlyOne()
        {
            // Arrange
            SessionsRepo repo = new SessionsRepo();

            var s1 = repo.Add(new Session { GroupId = 1, CarType = "Toy car" });
            var s2 = repo.Add(new Session { GroupId = 2, CarType = "Truck" });

            // Act
            repo.Delete(s1.Id);
            var all = repo.GetAll().ToList();

            // Assert
            Assert.Single(all);
            Assert.Equal(s2.Id, all[0].Id);
        }

        #endregion

        #region UPDATE TESTS

        [Fact]
        public void Update_IdDoesNotExist_ReturnsNull()
        {
            // Arrange
            SessionsRepo repo = new SessionsRepo();

            Session updatedSession = new Session
            {
                GroupId = 99,
                CarType = "Updated"
            };

            // Act
            Session? result = repo.Update(999, updatedSession);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Update_ExistingId_UpdatesSession()
        {
            // Arrange
            SessionsRepo repo = new SessionsRepo();

            Session added = repo.Add(new Session
            {
                GroupId = 1,
                CarType = "Toy car",
                RoadType = "Asphalt",
                SpeedLimit = 50,
                ScalingFactor = 10,
                Status = "Active",
                CreatedAt = DateTime.Now,
                EndedAt = null
            });

            Session updatedSession = new Session
            {
                GroupId = 2,
                CarType = "Truck",
                RoadType = "Gravel",
                SpeedLimit = 80,
                ScalingFactor = 20,
                Status = "Ended",
                CreatedAt = DateTime.Now.AddDays(1),
                EndedAt = DateTime.Now.AddDays(2)
            };

            // Act
            Session? result = repo.Update(added.Id, updatedSession);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.GroupId);
            Assert.Equal("Truck", result.CarType);
            Assert.Equal("Gravel", result.RoadType);
            Assert.Equal(80, result.SpeedLimit);
            Assert.Equal(20, result.ScalingFactor);
            Assert.Equal("Ended", result.Status);
            Assert.Equal(updatedSession.CreatedAt, result.CreatedAt);
            Assert.Equal(updatedSession.EndedAt, result.EndedAt);
        }

        [Fact]
        public void Update_ExistingId_DoesNotChangeId()
        {
            // Arrange
            SessionsRepo repo = new SessionsRepo();

            Session added = repo.Add(new Session { GroupId = 1 });

            Session updatedSession = new Session
            {
                Id = 999,
                GroupId = 2,
                CarType = "Truck"
            };

            // Act
            Session? result = repo.Update(added.Id, updatedSession);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(added.Id, result.Id);
            Assert.NotEqual(999, result.Id);
        }

        [Fact]
        public void Update_ExistingId_ReturnsCopy_NotSameReference()
        {
            // Arrange
            SessionsRepo repo = new SessionsRepo();

            Session added = repo.Add(new Session { GroupId = 1 });

            Session updatedSession = new Session { GroupId = 2 };

            // Act
            Session? result = repo.Update(added.Id, updatedSession);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(added, result);
            Assert.NotSame(updatedSession, result);
        }

        [Fact]
        public void Update_OneOfMultiple_UpdatesOnlySelectedSession()
        {
            // Arrange
            SessionsRepo repo = new SessionsRepo();

            Session first = repo.Add(new Session { GroupId = 1, CarType = "Toy car" });
            Session second = repo.Add(new Session { GroupId = 2, CarType = "Truck" });

            Session updatedSession = new Session
            {
                GroupId = 99,
                CarType = "Updated car"
            };

            // Act
            repo.Update(first.Id, updatedSession);

            // Assert
            Session? firstResult = repo.GetById(first.Id);
            Session? secondResult = repo.GetById(second.Id);

            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);

            Assert.Equal(99, firstResult.GroupId);
            Assert.Equal("Updated car", firstResult.CarType);

            Assert.Equal(2, secondResult.GroupId);
            Assert.Equal("Truck", secondResult.CarType);
        }

        #endregion
    }
}