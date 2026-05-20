using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FartmaalerAPI.Tests
{
    public class GroupsRepoTests
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
        // Tester at GetAll returnerer alle grupper
        public void GetAll_ReturnsAllGroups()
        {
            using var context = GetDbContext();

            context.Groups.AddRange(
                new Group
                {
                    Id = 1,
                    Name = "Gruppe 1",
                    School = "Roskilde Skole",
                    IsLocked = false
                },
                new Group
                {
                    Id = 2,
                    Name = "Gruppe 2",
                    School = "Roskilde Skole",
                    IsLocked = false
                });

            context.SaveChanges();

            var repo = new GroupsRepo(context);

            var result = repo.GetAll();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        // Tester at GetById returnerer gruppe hvis den findes
        public void GetById_ReturnsGroup_WhenGroupExists()
        {
            using var context = GetDbContext();

            context.Groups.Add(new Group
            {
                Id = 1,
                Name = "Gruppe 1",
                School = "Roskilde Skole",
                IsLocked = false
            });

            context.SaveChanges();

            var repo = new GroupsRepo(context);

            var result = repo.GetById(1);

            Assert.NotNull(result);
            Assert.Equal("Gruppe 1", result.Name);
        }

        [Fact]
        // Tester at GetById returnerer null hvis gruppen ikke findes
        public void GetById_ReturnsNull_WhenGroupDoesNotExist()
        {
            using var context = GetDbContext();

            var repo = new GroupsRepo(context);

            var result = repo.GetById(1);

            Assert.Null(result);
        }

        [Fact]
        // Tester at Add tilføjer gruppe korrekt
        public void Add_AddsGroup()
        {
            using var context = GetDbContext();

            var repo = new GroupsRepo(context);

            var group = new Group
            {
                Name = "Ny Gruppe",
                School = "Roskilde Skole",
                IsLocked = false
            };

            var result = repo.Add(group);

            Assert.NotNull(result);
            Assert.Single(context.Groups);
        }

        [Fact]
        // Tester at Delete returnerer null hvis gruppen ikke findes
        public void Delete_ReturnsNull_WhenGroupDoesNotExist()
        {
            using var context = GetDbContext();

            var repo = new GroupsRepo(context);

            var result = repo.Delete(1);

            Assert.Null(result);
        }

        [Fact]
        // Tester at Delete sletter gruppe korrekt
        public void Delete_RemovesGroup()
        {
            using var context = GetDbContext();

            context.Groups.Add(new Group
            {
                Id = 1,
                Name = "Gruppe 1",
                School = "Roskilde Skole",
                IsLocked = false
            });

            context.SaveChanges();

            var repo = new GroupsRepo(context);

            var result = repo.Delete(1);

            Assert.NotNull(result);
            Assert.Empty(context.Groups);
        }

        [Fact]
        // Tester at Update returnerer null hvis gruppen ikke findes
        public void Update_ReturnsNull_WhenGroupDoesNotExist()
        {
            using var context = GetDbContext();

            var repo = new GroupsRepo(context);

            var updatedGroup = new Group
            {
                Name = "Ny Gruppe",
                School = "Roskilde Skole",
                IsLocked = true
            };

            var result = repo.Update(1, updatedGroup);

            Assert.Null(result);
        }

        [Fact]
        // Tester at Update opdaterer gruppens værdier korrekt
        public void Update_UpdatesGroup()
        {
            using var context = GetDbContext();

            context.Groups.Add(new Group
            {
                Id = 1,
                Name = "Gammel Gruppe",
                School = "Roskilde Skole",
                IsLocked = false
            });

            context.SaveChanges();

            var repo = new GroupsRepo(context);

            var updatedGroup = new Group
            {
                Name = "Ny Gruppe",
                School = "Ny Skole",
                IsLocked = true
            };

            var result = repo.Update(1, updatedGroup);

            Assert.NotNull(result);
            Assert.Equal("Ny Gruppe", result.Name);
            Assert.Equal("Ny Skole", result.School);
            Assert.True(result.IsLocked);
        }
    }
}