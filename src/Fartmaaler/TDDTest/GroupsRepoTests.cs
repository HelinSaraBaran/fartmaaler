using System;
using System.Linq;
using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TDDTest
{
    // Tester GroupsRepo, så vi ved at CRUD metoderne virker korrekt
    // Testene bruger en InMemory database, så vi ikke rammer den rigtige database
    public class GroupsRepoTests : IDisposable
    {
        // Test database context
        private readonly AppDbContext _context;

        // Repository som vi tester
        private readonly GroupsRepo _repo;

        // Constructoren kører før hver test
        public GroupsRepoTests()
        {
            // Opretter en unik InMemory database til hver test
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Opretter context med test databasen
            _context = new AppDbContext(options);

            // Opretter repository med test context
            _repo = new GroupsRepo(_context);
        }

        // Dispose kører efter hver test
        // Den rydder context op efter testen
        public void Dispose()
        {
            _context.Dispose();
        }

        // Hjælpemetode der opretter en standard test-gruppe
        // Gør testene kortere og undgår gentaget kode
        private Group CreateGroup(string name = "Test Group")
        {
            return new Group
            {
                Name = name,
                School = "Test School",
                IsLocked = false
            };
        }

        #region GET TESTS

        [Fact]
        public void GetAll_WhenEmpty_ReturnsEmptyList()
        {
            // Act - henter alle grupper fra en tom database
            var result = _repo.GetAll();

            // Assert -tjekker at listen er tom
            Assert.Empty(result);
        }

        [Fact]
        public void GetAll_WhenGroupsExist_ReturnsAll()
        {
            // Arrange - tilføjer tre grupper direkte i test databasen
            _context.Groups.AddRange(
                CreateGroup("A"),
                CreateGroup("B"),
                CreateGroup("C")
            );
            _context.SaveChanges();

            // Act - henter alle grupper via repository
            var result = _repo.GetAll().ToList();

            // Assert - tjekker at alle tre grupper bliver returneret
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetById_WhenExists_ReturnsGroup()
        {
            // Arrange: opretter og gemmer en gruppe
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            // Act: henter gruppen ud fra id
            var result = _repo.GetById(group.Id);

            // Assert -  tjekker at gruppen findes og har korrekt id
            Assert.NotNull(result);
            Assert.Equal(group.Id, result.Id);
        }

        [Fact]
        public void GetById_WhenNotExists_ReturnsNull()
        {
            // Act - forsøger at hente en gruppe med et id der ikke findes
            var result = _repo.GetById(999);

            // Assert - tjekker at resultatet er null
            Assert.Null(result);
        }

        #endregion

        #region ADD TESTS

        [Fact]
        public void Add_ValidGroup_AddsToDatabase()
        {
            // Arrange -  opretter en gyldig gruppe
            var group = CreateGroup();

            // Act - tilføjer gruppen via repository
            var result = _repo.Add(group);

            // Assert - tjekker at gruppen blev gemt og fik et id
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Single(_context.Groups);
        }

        [Fact]
        public void Add_ValidGroup_PropertiesSavedCorrectly()
        {
            // Arrange -  opretter en gruppe med bestemt navn
            var group = CreateGroup("My Group");

            // Act - gemmer gruppen
            var result = _repo.Add(group);

            // Assert - tjekker at værdierne blev gemt korrekt
            Assert.Equal("My Group", result.Name);
            Assert.Equal("Test School", result.School);
            Assert.False(result.IsLocked);
        }

        [Fact]
        public void Add_Group_CanBeRetrievedById()
        {
            // Arrange - opretter en gruppe
            var group = CreateGroup();

            // Act -  gemmer gruppen og henter den igen
            var added = _repo.Add(group);
            var found = _repo.GetById(added.Id);

            // Assert -  tjekker at den gemte gruppe kan findes igen
            Assert.NotNull(found);
            Assert.Equal(added.Id, found.Id);
        }

        #endregion

        #region DELETE TESTS

        [Fact]
        public void Delete_WhenNotExists_ReturnsNull()
        {
            // Act - forsøger at slette en gruppe der ikke findes
            var result = _repo.Delete(999);

            // Assert -  tjekker at metoden returnerer null
            Assert.Null(result);
        }

        [Fact]
        public void Delete_WhenExists_RemovesGroup()
        {
            // Arrange -  opretter en gruppe
            var group = _repo.Add(CreateGroup());

            // Act - sletter gruppen og henter alle grupper
            var deleted = _repo.Delete(group.Id);
            var all = _repo.GetAll();

            // Assert -  tjekker at gruppen blev slettet
            Assert.NotNull(deleted);
            Assert.Empty(all);
        }

        [Fact]
        public void Delete_WhenExists_ReturnsDeletedGroup()
        {
            // Arrange-  opretter en gruppe
            var group = _repo.Add(CreateGroup());

            // Act - sletter gruppen
            var result = _repo.Delete(group.Id);

            // Assert - tjekker at den slettede gruppe returneres
            Assert.NotNull(result);
            Assert.Equal(group.Id, result.Id);
        }

        [Fact]
        public void Delete_OneOfMultiple_RemovesOnlyOne()
        {
            // Arrange -  opretter to grupper
            var g1 = _repo.Add(CreateGroup("A"));
            var g2 = _repo.Add(CreateGroup("B"));

            // Act -  sletter kun den første gruppe
            _repo.Delete(g1.Id);
            var all = _repo.GetAll().ToList();

            // Assert -  tjekker at kun en gruppe er tilbage
            Assert.Single(all);
            Assert.Equal(g2.Id, all[0].Id);
        }

        #endregion

        #region UPDATE TESTS

        [Fact]
        public void Update_WhenNotExists_ReturnsNull()
        {
            // Arrange - opretter nye værdier til en gruppe
            var updated = CreateGroup("Updated");

            // Act-  forsøger at opdatere en gruppe der ikke findes
            var result = _repo.Update(999, updated);

            // Assert - tjekker at resultatet er null
            Assert.Null(result);
        }

        [Fact]
        public void Update_WhenExists_UpdatesGroup()
        {
            // Arrange - opretter en gruppe
            var group = _repo.Add(CreateGroup());

            // Arrange - opretter nye værdier
            var updated = new Group
            {
                Name = "Updated Name",
                School = "New School",
                IsLocked = true
            };

            // Act - opdaterer gruppen
            var result = _repo.Update(group.Id, updated);

            // Assert -  tjekker at felterne blev opdateret korrekt
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal("New School", result.School);
            Assert.True(result.IsLocked);
        }

        [Fact]
        public void Update_WhenExists_DoesNotChangeId()
        {
            // Arrange - opretter en gruppe
            var group = _repo.Add(CreateGroup());

            // Arrange - laver opdateret objekt med et andet id
            var updated = CreateGroup("New");
            updated.Id = 999;

            // Act - opdaterer gruppen
            var result = _repo.Update(group.Id, updated);

            // Assert - tjekker at originalt id ikke bliver ændret
            Assert.NotNull(result);
            Assert.Equal(group.Id, result.Id);
            Assert.NotEqual(999, result.Id);
        }

        [Fact]
        public void Update_OneOfMultiple_UpdatesOnlySelected()
        {
            // Arrange -  opretter to grupper
            var g1 = _repo.Add(CreateGroup("A"));
            var g2 = _repo.Add(CreateGroup("B"));

            // Arrange -  nye værdier til opdatering
            var updated = new Group
            {
                Name = "Updated",
                School = "School",
                IsLocked = true
            };

            // Act -  opdaterer kun den første gruppe
            _repo.Update(g1.Id, updated);

            // Act -  henter begge grupper igen
            var first = _repo.GetById(g1.Id);
            var second = _repo.GetById(g2.Id);

            // Assert -  tjekker at kun den valgte gruppe blev opdateret
            Assert.Equal("Updated", first.Name);
            Assert.Equal("B", second.Name);
        }

        #endregion
    }
}