using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace TDDTest
{
    // Tester MeasurementsRepo
    // Bruger InMemory database, så vi ikke rammer rigtig database
    public class MeasurementsRepoTests : IDisposable
    {
        // Test database context
        private readonly AppDbContext _context;

        // Repository der testes
        private readonly MeasurementsRepo _repo;

        // Kører før hver test
        public MeasurementsRepoTests()
        {
            // Opretter en unik InMemory database for hver test
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Initialiserer context og repo
            _context = new AppDbContext(options);
            _repo = new MeasurementsRepo(_context);
        }

        // Rydder op efter hver test
        public void Dispose()
        {
            _context.Dispose();
        }

        // Hjælpemetode til at oprette en test måling
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
            // Act - henter alle målinger
            var result = _repo.GetAll();

            // Assert - forventer tom liste
            Assert.Empty(result);
        }

        [Fact]
        public void GetAll_WhenMeasurementsExist_ReturnsAllMeasurements()
        {
            // Arrange - tilføjer 3 målinger
            _context.Measurements.AddRange(
                CreateMeasurement(),
                CreateMeasurement(),
                CreateMeasurement()
            );
            _context.SaveChanges();

            // Act -  henter alle målinger
            var result = _repo.GetAll().ToList();

            // Assert - forventer 3 målinger
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetById_WhenIdExists_ReturnsMeasurement()
        {
            // Arrange -  opretter en måling
            var measurement = CreateMeasurement();
            _context.Measurements.Add(measurement);
            _context.SaveChanges();

            // Act - henter målingen via id
            var result = _repo.GetById(measurement.Id);

            // Assert - målingen findes og har korrekt id
            Assert.NotNull(result);
            Assert.Equal(measurement.Id, result.Id);
        }

        [Fact]
        public void GetById_WhenIdDoesNotExist_ReturnsNull()
        {
            // Act - forsøger at hente en måling der ikke findes
            var result = _repo.GetById(999);

            // Assert - forventer null
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

            // Assert - målingen er gemt og har fået id
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

            // Assert - alle værdier er gemt korrekt
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

            // Assert - målingen kan findes igen
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

            // Assert - databasen er tom efter sletning
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

            // Assert -  korrekt måling returneres
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

            // Assert -  kun en tilbage
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

            // Act + Assert - forventer exception fordi update ikke er tilladt
            Assert.Throws<NotImplementedException>(() =>
                _repo.Update(999, updatedMeasurement));
        }

        #endregion
    }
}