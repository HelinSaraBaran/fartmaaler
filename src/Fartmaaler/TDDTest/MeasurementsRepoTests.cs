using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.Metrics;
using System.Linq;
using Xunit;

namespace TDDTest
{
    public class MeasurementsRepoTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly MeasurementsRepo _repo;

        public MeasurementsRepoTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repo = new MeasurementsRepo(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        private Measurement CreateMeasurement()
        {
            return new Measurement
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
        }

        #region GET TESTS

        [Fact]
        public void GetAll_WhenRepoIsEmpty_ReturnsEmptyList()
        {
            // Act
            var result = _repo.GetAll();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetAll_WhenMeasurementsExist_ReturnsAllMeasurements()
        {
            // Arrange
            _context.Measurements.AddRange(
                CreateMeasurement(),
                CreateMeasurement(),
                CreateMeasurement()
            );
            _context.SaveChanges();

            // Act
            var result = _repo.GetAll().ToList();

            // Assert
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetById_WhenIdExists_ReturnsMeasurement()
        {
            // Arrange
            var measurement = CreateMeasurement();

            _context.Measurements.Add(measurement);
            _context.SaveChanges();

            // Act
            var result = _repo.GetById(measurement.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(measurement.Id, result.Id);
        }

        [Fact]
        public void GetById_WhenIdDoesNotExist_ReturnsNull()
        {
            // Act
            var result = _repo.GetById(999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ADD TESTS

        [Fact]
        public void Add_ValidMeasurement_AddsMeasurementToDatabase()
        {
            // Arrange
            var measurement = CreateMeasurement();

            // Act
            var result = _repo.Add(measurement);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Single(_context.Measurements);
        }

        [Fact]
        public void Add_ValidMeasurement_PropertiesAreSavedCorrectly()
        {
            // Arrange
            var measurement = CreateMeasurement();

            // Act
            var result = _repo.Add(measurement);

            // Assert
            Assert.Equal(1, result.SessionId);
            Assert.Equal(10, result.MeasuredSpeed);
            Assert.Equal(100, result.SimulatedSpeed);
            Assert.Equal(2, result.Time);
            Assert.Equal(5, result.Distance);
            Assert.Equal(20, result.Co2);
            Assert.Equal(5, result.Co2Saved);
        }

        [Fact]
        public void Add_ValidMeasurement_CanBeFoundById()
        {
            // Arrange
            var measurement = CreateMeasurement();

            // Act
            var added = _repo.Add(measurement);
            var found = _repo.GetById(added.Id);

            // Assert
            Assert.NotNull(found);
            Assert.Equal(added.Id, found.Id);
        }

        #endregion

        #region DELETE TESTS

        [Fact]
        public void Delete_WhenIdDoesNotExist_ReturnsNull()
        {
            // Act
            var result = _repo.Delete(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Delete_WhenIdExists_RemovesMeasurementFromDatabase()
        {
            // Arrange
            var measurement = _repo.Add(CreateMeasurement());

            // Act
            var deleted = _repo.Delete(measurement.Id);
            var all = _repo.GetAll();

            // Assert
            Assert.NotNull(deleted);
            Assert.Empty(all);
        }

        [Fact]
        public void Delete_WhenIdExists_ReturnsDeletedMeasurement()
        {
            // Arrange
            var measurement = _repo.Add(CreateMeasurement());

            // Act
            var result = _repo.Delete(measurement.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(measurement.Id, result.Id);
            Assert.Equal(10, result.MeasuredSpeed);
        }

        [Fact]
        public void Delete_OneOfMultiple_RemovesOnlySelectedMeasurement()
        {
            // Arrange
            var m1 = _repo.Add(CreateMeasurement());
            var m2 = _repo.Add(CreateMeasurement());

            // Act
            _repo.Delete(m1.Id);
            var all = _repo.GetAll().ToList();

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
            var updatedMeasurement = CreateMeasurement();

            // Act + Assert
            Assert.Throws<NotImplementedException>(() =>
                _repo.Update(999, updatedMeasurement));
        }

        #endregion
    }
}