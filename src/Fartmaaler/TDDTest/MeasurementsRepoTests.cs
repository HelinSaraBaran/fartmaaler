using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
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
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repo = new MeasurementsRepo(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        private Measurement CreateMeasurement(int sessionId = 1)
        {
            return new Measurement
            {
                SessionId = sessionId,
                MeasuredSpeed = 18,
                SimulatedSpeed = 50,
                Time = 1,
                Distance = 5,
                SpeedLimit = 50,
                Status = "On limit",
                Co2 = 0,
                Co2Saved = 0,
                CreatedAt = DateTime.Now
            };
        }

        #region GET TESTS

        [Fact]
        public void GetAll_WhenEmpty_ReturnsEmptyList()
        {
            var result = _repo.GetAll();

            Assert.Empty(result);
        }

        [Fact]
        public void GetAll_WhenMeasurementsExist_ReturnsAllMeasurements()
        {
            _repo.Add(CreateMeasurement());
            _repo.Add(CreateMeasurement());
            _repo.Add(CreateMeasurement());

            var result = _repo.GetAll().ToList();

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetById_WhenExists_ReturnsMeasurement()
        {
            var measurement = _repo.Add(CreateMeasurement());

            var result = _repo.GetById(measurement.Id);

            Assert.NotNull(result);
            Assert.Equal(measurement.Id, result.Id);
        }

        [Fact]
        public void GetById_WhenNotExists_ReturnsNull()
        {
            var result = _repo.GetById(999);

            Assert.Null(result);
        }

        [Fact]
        public void GetBySessionId_WhenMeasurementsExist_ReturnsOnlyMeasurementsForSession()
        {
            _repo.Add(CreateMeasurement(1));
            _repo.Add(CreateMeasurement(1));
            _repo.Add(CreateMeasurement(2));

            var result = _repo.GetBySessionId(1).ToList();

            Assert.Equal(2, result.Count);
            Assert.All(result, m => Assert.Equal(1, m.SessionId));
        }

        [Fact]
        public void GetBySessionId_WhenNoMeasurementsExist_ReturnsEmptyList()
        {
            var result = _repo.GetBySessionId(999);

            Assert.Empty(result);
        }

        #endregion

        #region ADD TESTS

        [Fact]
        public void Add_ValidMeasurement_SavesMeasurement()
        {
            var measurement = CreateMeasurement();

            var result = _repo.Add(measurement);

            Assert.True(result.Id > 0);
            Assert.Single(_context.Measurements);
        }

        [Fact]
        public void Add_ValidMeasurement_PropertiesAreSavedCorrectly()
        {
            var result = _repo.Add(CreateMeasurement());

            Assert.Equal(1, result.SessionId);
            Assert.Equal(18, result.MeasuredSpeed);
            Assert.Equal(50, result.SimulatedSpeed);
            Assert.Equal(1, result.Time);
            Assert.Equal(5, result.Distance);
            Assert.Equal(50, result.SpeedLimit);
            Assert.Equal("On limit", result.Status);
            Assert.Equal(0, result.Co2);
            Assert.Equal(0, result.Co2Saved);
        }

        [Fact]
        public void Add_ValidMeasurement_CanBeFoundById()
        {
            var measurement = _repo.Add(CreateMeasurement());

            var found = _repo.GetById(measurement.Id);

            Assert.NotNull(found);
            Assert.Equal(measurement.Id, found.Id);
        }

        #endregion

        #region DELETE TESTS

        [Fact]
        public void Delete_WhenExists_RemovesMeasurement()
        {
            var measurement = _repo.Add(CreateMeasurement());

            var result = _repo.Delete(measurement.Id);

            Assert.NotNull(result);
            Assert.Empty(_context.Measurements);
        }

        [Fact]
        public void Delete_WhenExists_ReturnsDeletedMeasurement()
        {
            var measurement = _repo.Add(CreateMeasurement());

            var result = _repo.Delete(measurement.Id);

            Assert.NotNull(result);
            Assert.Equal(measurement.Id, result.Id);
            Assert.Equal(18, result.MeasuredSpeed);
        }

        [Fact]
        public void Delete_WhenNotExists_ReturnsNull()
        {
            var result = _repo.Delete(999);

            Assert.Null(result);
        }

        #endregion

        #region UPDATE TESTS

        [Fact]
        public void Update_WhenCalled_ThrowsException()
        {
            Assert.Throws<NotImplementedException>(() =>
                _repo.Update(1, CreateMeasurement()));
        }

        #endregion
    }
}