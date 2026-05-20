using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FartmaalerAPI.Tests
{
    public class MeasurementsRepoTests
    {
        // Opretter fake in-memory database
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        // Tester at GetAll returnerer alle målinger
        public void GetAll_ReturnsAllMeasurements()
        {
            using var context = GetDbContext();

            context.Measurements.AddRange(
                new Measurement
                {
                    Id = 1,
                    SessionId = 1,
                    MeasuredSpeed = 10,
                    SimulatedSpeed = 50,
                    Time = 2,
                    Distance = 5,
                    SpeedLimit = 50,
                    Status = "On limit",
                    Co2 = 2,
                    Co2Saved = 1,
                    CreatedAt = DateTime.Now
                },
                new Measurement
                {
                    Id = 2,
                    SessionId = 1,
                    MeasuredSpeed = 12,
                    SimulatedSpeed = 60,
                    Time = 2,
                    Distance = 5,
                    SpeedLimit = 50,
                    Status = "Too fast",
                    Co2 = 3,
                    Co2Saved = 0,
                    CreatedAt = DateTime.Now
                });

            context.SaveChanges();

            var repo = new MeasurementsRepo(context);

            var result = repo.GetAll();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        // Tester at GetById returnerer måling hvis den findes
        public void GetById_ReturnsMeasurement_WhenMeasurementExists()
        {
            using var context = GetDbContext();

            context.Measurements.Add(new Measurement
            {
                Id = 1,
                SessionId = 1,
                MeasuredSpeed = 10,
                SimulatedSpeed = 50,
                Time = 2,
                Distance = 5,
                SpeedLimit = 50,
                Status = "On limit",
                Co2 = 2,
                Co2Saved = 1,
                CreatedAt = DateTime.Now
            });

            context.SaveChanges();

            var repo = new MeasurementsRepo(context);

            var result = repo.GetById(1);

            Assert.NotNull(result);
            Assert.Equal(50, result.SimulatedSpeed);
        }

        [Fact]
        // Tester at GetById returnerer null hvis målingen ikke findes
        public void GetById_ReturnsNull_WhenMeasurementDoesNotExist()
        {
            using var context = GetDbContext();

            var repo = new MeasurementsRepo(context);

            var result = repo.GetById(1);

            Assert.Null(result);
        }

        [Fact]
        // Tester at GetBySessionId returnerer målinger for korrekt session
        public void GetBySessionId_ReturnsMeasurementsForSession()
        {
            using var context = GetDbContext();

            context.Measurements.AddRange(
                new Measurement
                {
                    Id = 1,
                    SessionId = 1,
                    MeasuredSpeed = 10,
                    SimulatedSpeed = 50,
                    Time = 2,
                    Distance = 5,
                    SpeedLimit = 50,
                    Status = "On limit",
                    Co2 = 2,
                    Co2Saved = 1,
                    CreatedAt = DateTime.Now
                },
                new Measurement
                {
                    Id = 2,
                    SessionId = 2,
                    MeasuredSpeed = 12,
                    SimulatedSpeed = 60,
                    Time = 2,
                    Distance = 5,
                    SpeedLimit = 50,
                    Status = "Too fast",
                    Co2 = 3,
                    Co2Saved = 0,
                    CreatedAt = DateTime.Now
                });

            context.SaveChanges();

            var repo = new MeasurementsRepo(context);

            var result = repo.GetBySessionId(1);

            Assert.Single(result);
        }

        [Fact]
        // Tester at Add tilføjer måling korrekt
        public void Add_AddsMeasurement()
        {
            using var context = GetDbContext();

            var repo = new MeasurementsRepo(context);

            var measurement = new Measurement
            {
                SessionId = 1,
                MeasuredSpeed = 10,
                SimulatedSpeed = 50,
                Time = 2,
                Distance = 5,
                SpeedLimit = 50,
                Status = "On limit",
                Co2 = 2,
                Co2Saved = 1,
                CreatedAt = DateTime.Now
            };

            var result = repo.Add(measurement);

            Assert.NotNull(result);
            Assert.Single(context.Measurements);
        }

        [Fact]
        // Tester at Delete returnerer null hvis målingen ikke findes
        public void Delete_ReturnsNull_WhenMeasurementDoesNotExist()
        {
            using var context = GetDbContext();

            var repo = new MeasurementsRepo(context);

            var result = repo.Delete(1);

            Assert.Null(result);
        }

        [Fact]
        // Tester at Delete sletter måling korrekt
        public void Delete_RemovesMeasurement()
        {
            using var context = GetDbContext();

            context.Measurements.Add(new Measurement
            {
                Id = 1,
                SessionId = 1,
                MeasuredSpeed = 10,
                SimulatedSpeed = 50,
                Time = 2,
                Distance = 5,
                SpeedLimit = 50,
                Status = "On limit",
                Co2 = 2,
                Co2Saved = 1,
                CreatedAt = DateTime.Now
            });

            context.SaveChanges();

            var repo = new MeasurementsRepo(context);

            var result = repo.Delete(1);

            Assert.NotNull(result);
            Assert.Empty(context.Measurements);
        }

        [Fact]
        // Tester at Update kaster exception fordi målinger ikke kan opdateres
        public void Update_ThrowsException()
        {
            using var context = GetDbContext();

            var repo = new MeasurementsRepo(context);

            var measurement = new Measurement
            {
                SessionId = 1,
                MeasuredSpeed = 10,
                SimulatedSpeed = 50,
                Time = 2,
                Distance = 5,
                SpeedLimit = 50,
                Status = "On limit",
                Co2 = 2,
                Co2Saved = 1,
                CreatedAt = DateTime.Now
            };

            Assert.Throws<NotImplementedException>(() =>
                repo.Update(1, measurement));
        }
    }
}