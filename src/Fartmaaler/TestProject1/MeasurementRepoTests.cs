using FartmaalerAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using FartmaalerAPI.Models;


namespace TestProject1
{
    public class MeasurementRepoTests
    {
        #region GET TESTS
        [Fact]
        public void GetAll_WhenRepoIsEmpty_ReturnsEmptyList()
        {
            // Arrange
            MeasurementsRepo repo = new MeasurementsRepo();

            // Act
            var result = repo.GetAll();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetById_WhenIdDoesNotExist_ReturnsNull()
        {
            // Arrange
            MeasurementsRepo repo = new MeasurementsRepo();

            // Act
            var result = repo.GetById(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region ADD TESTS
        [Fact] // This test assumes that the Add method assigns an Id to the measurement and that the Id is greater than 0.
        public void Add_ValidMeasurement_ReturnsMeasurementWithId()
        {
            // Arrange
            MeasurementsRepo repo = new MeasurementsRepo();

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
        [Fact] // This test checks that the properties of the measurement are correctly copied when added to the repository.
        public void Add_ValidMeasurement_PropertiesAreCopiedCorrectly()
        {
            // Arrange
            MeasurementsRepo repo = new MeasurementsRepo();

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
        [Fact] // This test checks that after adding a measurement, it can be retrieved from the repository and that the list of all measurements contains the added measurement.
        public void Add_ValidMeasurement_IsAddedToList()
        {
            // Arrange
            MeasurementsRepo repo = new MeasurementsRepo();

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
            var all = repo.GetAll();

            // Assert
            Assert.Single(all);
            Assert.Equal(added.Id, all.First().Id);
        }
        [Fact] // This test checks that the Add method returns a new instance of Measurement and not the same reference that was passed in.
        public void Add_ReturnsCopy_NotSameReference()
        {
            // Arrange
            MeasurementsRepo repo = new MeasurementsRepo();

            Measurement m = new Measurement
            {
                SessionId = 1
            };

            // Act
            var result = repo.Add(m);

            // Assert
            Assert.NotSame(m, result);
        }
        #endregion

        #region DELETE TESTS
        [Fact] // This test checks that the Delete method returns null when trying to delete a measurement with an Id that does not exist in the repository.
        public void Delete_IdDoesNotExist_ReturnsNull()
        {
            // Arrange
            MeasurementsRepo repo = new MeasurementsRepo();

            // Act
            var result = repo.Delete(999);

            // Assert
            Assert.Null(result);
        }
        [Fact] // This test checks that after adding a measurement and then deleting it, the list of all measurements is empty, confirming that the measurement was successfully removed from the repository.
        public void Delete_ExistingId_RemovesMeasurement()
        {
            // Arrange
            MeasurementsRepo repo = new MeasurementsRepo();

            var m = new Measurement
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
        [Fact] // This test checks that the Delete method returns the correct measurement that was deleted, confirming that the method is correctly identifying and removing the measurement based on its Id.
        public void Delete_ExistingId_ReturnsDeletedMeasurement()
        {
            // Arrange
            MeasurementsRepo repo = new MeasurementsRepo();

            var m = new Measurement
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
        [Fact] // This test checks that the Delete method returns a new instance of Measurement and not the same reference that was added to the repository, confirming that the method is correctly creating a copy of the deleted measurement before returning it.
        public void Delete_ReturnsCopy_NotSameReference()
        {
            // Arrange
            MeasurementsRepo repo = new MeasurementsRepo();

            var m = new Measurement { SessionId = 1 };
            var added = repo.Add(m);

            // Act
            var result = repo.Delete(added.Id);

            // Assert
            Assert.NotSame(added, result);
        }
        [Fact] // This test checks that when there are multiple measurements in the repository and one of them is deleted, only the specified measurement is removed and the others remain intact, confirming that the Delete method is correctly identifying and removing only the intended measurement based on its Id.
        public void Delete_OneOfMultiple_RemovesOnlyOne()
        {
            // Arrange
            MeasurementsRepo repo = new MeasurementsRepo();

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

        [Fact]// This test checks that when trying to update a measurement with an Id that does not exist in the repository, the Update method returns null, confirming that the method is correctly handling cases where the specified Id is not found.
        public void Update_IdDoesNotExist_ReturnsNull()
        {
            // Arrange
            MeasurementsRepo repo = new MeasurementsRepo();

            Measurement updatedMeasurement = new Measurement
            {
                SessionId = 99
            };

            // Act
            Measurement? result = repo.Update(999, updatedMeasurement);

            // Assert
            Assert.Null(result);
        }

        [Fact] // This test checks that when updating an existing measurement with a valid Id, the Update method correctly updates the properties of the measurement and returns the updated measurement, confirming that the method is functioning as intended for valid update operations.
        public void Update_ExistingId_UpdatesMeasurement()
        {
            // Arrange
            MeasurementsRepo repo = new MeasurementsRepo();

            Measurement added = repo.Add(new Measurement
            {
                SessionId = 1,
                MeasuredSpeed = 10,
                SimulatedSpeed = 100,
                Time = 2,
                Distance = 5,
                Co2 = 20,
                Co2Saved = 5,
                CreatedAt = DateTime.Now
            });

            Measurement updatedMeasurement = new Measurement
            {
                SessionId = 2,
                MeasuredSpeed = 50,
                SimulatedSpeed = 120,
                Time = 3,
                Distance = 10,
                Co2 = 30,
                Co2Saved = 15,
                CreatedAt = DateTime.Now.AddDays(1)
            };

            // Act
            Measurement? result = repo.Update(added.Id, updatedMeasurement);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.SessionId);
            Assert.Equal(50, result.MeasuredSpeed);
            Assert.Equal(120, result.SimulatedSpeed);
            Assert.Equal(3, result.Time);
            Assert.Equal(10, result.Distance);
            Assert.Equal(30, result.Co2);
            Assert.Equal(15, result.Co2Saved);
            Assert.Equal(updatedMeasurement.CreatedAt, result.CreatedAt);
        }
        [Fact] // This test checks that when updating an existing measurement, the Id of the measurement remains unchanged even if the updatedMeasurement object has a different Id value, confirming that the Update method correctly preserves the original Id of the measurement being updated.
        public void Update_ExistingId_DoesNotChangeId()
        {
            // Arrange
            MeasurementsRepo repo = new MeasurementsRepo();

            Measurement added = repo.Add(new Measurement { SessionId = 1 });

            Measurement updatedMeasurement = new Measurement
            {
                Id = 999,
                SessionId = 2
            };

            // Act
            Measurement? result = repo.Update(added.Id, updatedMeasurement);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(added.Id, result.Id);
            Assert.NotEqual(999, result.Id);
        }

        [Fact] // This test checks that when updating an existing measurement, the Update method returns a new instance of Measurement and not the same reference as either the original measurement or the updatedMeasurement object, confirming that the method is correctly creating a copy of the updated measurement before returning it.
        public void Update_ExistingId_ReturnsCopy_NotSameReference()
        {
            // Arrange
            MeasurementsRepo repo = new MeasurementsRepo();

            Measurement added = repo.Add(new Measurement { SessionId = 1 });

            Measurement updatedMeasurement = new Measurement { SessionId = 2 };

            // Act
            Measurement? result = repo.Update(added.Id, updatedMeasurement);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(added, result);
            Assert.NotSame(updatedMeasurement, result);
        }
        [Fact] // This test checks that when there are multiple measurements in the repository and one of them is updated, only the specified measurement is updated and the others remain unchanged, confirming that the Update method is correctly identifying and updating only the intended measurement based on its Id.
        public void Update_OneOfMultiple_UpdatesOnlySelectedMeasurement()
        {
            // Arrange
            MeasurementsRepo repo = new MeasurementsRepo();

            Measurement first = repo.Add(new Measurement { SessionId = 1 });
            Measurement second = repo.Add(new Measurement { SessionId = 2 });

            Measurement updatedMeasurement = new Measurement
            {
                SessionId = 99
            };

            // Act
            repo.Update(first.Id, updatedMeasurement);

            // Assert
            Measurement? firstResult = repo.GetById(first.Id);
            Measurement? secondResult = repo.GetById(second.Id);

            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);

            Assert.Equal(99, firstResult.SessionId);
            Assert.Equal(2, secondResult.SessionId);
        }
        #endregion
    }
}

