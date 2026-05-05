using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;


namespace TestProject1
{
    public class MeasurementRepoTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private MeasurementsRepo GetRepo()
        {
            AppDbContext context = GetInMemoryDbContext();
            return new MeasurementsRepo(context);
        }

        #region GET TESTS

        [Fact]
        public void GetAll_WhenRepoIsEmpty_ReturnsEmptyList()
        {
            // Arrange
            MeasurementsRepo repo = GetRepo();

            // Act
            var result = repo.GetAll();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetById_WhenIdDoesNotExist_ReturnsNull()
        {
            // Arrange
            MeasurementsRepo repo = GetRepo();

            // Act
            var result = repo.GetById(999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ADD TESTS

        [Fact]
        public void Add_ValidMeasurement_ReturnsMeasurementWithId()
        {
            // Arrange
            MeasurementsRepo repo = GetRepo();

            Measurement m = new Measurement
            {
                SessionId = 1,
                MeasuredSpeed = 10,
                SimulatedSpeed = 100,
                Time = 2,
                Distance = 5,
                Co2 = 20,
                Co2Saved = 5,
                CreatedAt = DateTime.Now
            };

            // Act
            var result = repo.Add(m);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
        }

        [Fact]
        public void Add_ValidMeasurement_PropertiesAreSavedCorrectly()
        {
            // Arrange
            MeasurementsRepo repo = GetRepo();

            Measurement m = new Measurement
            {
                SessionId = 2,
                MeasuredSpeed = 50,
                SimulatedSpeed = 120,
                Time = 3,
                Distance = 10,
                Co2 = 30,
                Co2Saved = 10,
                CreatedAt = DateTime.Now
            };

            // Act
            var result = repo.Add(m);

            // Assert
            Assert.Equal(m.SessionId, result.SessionId);
            Assert.Equal(m.MeasuredSpeed, result.MeasuredSpeed);
            Assert.Equal(m.SimulatedSpeed, result.SimulatedSpeed);
            Assert.Equal(m.Time, result.Time);
            Assert.Equal(m.Distance, result.Distance);
            Assert.Equal(m.Co2, result.Co2);
            Assert.Equal(m.Co2Saved, result.Co2Saved);
        }

        [Fact]
        public void Add_ValidMeasurement_IsAddedToDatabase()
        {
            // Arrange
            MeasurementsRepo repo = GetRepo();

            Measurement m = new Measurement
            {
                SessionId = 1,
                MeasuredSpeed = 10,
                SimulatedSpeed = 100,
                Time = 2,
                Distance = 5,
                Co2 = 20,
                Co2Saved = 5,
                CreatedAt = DateTime.Now
            };

            // Act
            var added = repo.Add(m);
            var all = repo.GetAll().ToList();

            // Assert
            Assert.Single(all);
            Assert.Equal(added.Id, all.First().Id);
        }

        #endregion

        #region DELETE TESTS

        [Fact]
        public void Delete_IdDoesNotExist_ReturnsNull()
        {
            // Arrange
            MeasurementsRepo repo = GetRepo();

            // Act
            var result = repo.Delete(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Delete_ExistingId_RemovesMeasurement()
        {
            // Arrange
            MeasurementsRepo repo = GetRepo();

            Measurement m = new Measurement
            {
                SessionId = 1
            };

            var added = repo.Add(m);

            // Act
            repo.Delete(added.Id);
            var all = repo.GetAll();

            // Assert
            Assert.Empty(all);
        }

        [Fact]
        public void Delete_ExistingId_ReturnsDeletedMeasurement()
        {
            // Arrange
            MeasurementsRepo repo = GetRepo();

            Measurement m = new Measurement
            {
                SessionId = 2,
                MeasuredSpeed = 50
            };

            var added = repo.Add(m);

            // Act
            var result = repo.Delete(added.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(added.Id, result.Id);
            Assert.Equal(2, result.SessionId);
            Assert.Equal(50, result.MeasuredSpeed);
        }

        [Fact]
        public void Delete_OneOfMultiple_RemovesOnlyOne()
        {
            // Arrange
            MeasurementsRepo repo = GetRepo();

            var m1 = repo.Add(new Measurement { SessionId = 1 });
            var m2 = repo.Add(new Measurement { SessionId = 2 });

            // Act
            repo.Delete(m1.Id);
            var all = repo.GetAll().ToList();

            // Assert
            Assert.Single(all);
            Assert.Equal(m2.Id, all[0].Id);
        }

        #endregion

        #region UPDATE TESTS

        [Fact]
        public void Update_WhenCalled_ThrowsNotImplementedException()
        {
            // Arrange
            MeasurementsRepo repo = GetRepo();

            Measurement updatedMeasurement = new Measurement
            {
                SessionId = 99
            };

            // Act + Assert
            Assert.Throws<NotImplementedException>(() =>
                repo.Update(999, updatedMeasurement));
        }

        #endregion
    }
}
