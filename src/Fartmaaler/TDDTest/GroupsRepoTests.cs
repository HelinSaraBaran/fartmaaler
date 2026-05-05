using System;
using System.Linq;
using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TDDTest
{
    public class GroupsRepoTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly GroupsRepo _repo;

        public GroupsRepoTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repo = new GroupsRepo(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

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
            var result = _repo.GetAll();

            Assert.Empty(result);
        }

        [Fact]
        public void GetAll_WhenGroupsExist_ReturnsAll()
        {
            _context.Groups.AddRange(
                CreateGroup("A"),
                CreateGroup("B"),
                CreateGroup("C")
            );
            _context.SaveChanges();

            var result = _repo.GetAll().ToList();

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetById_WhenExists_ReturnsGroup()
        {
            var group = CreateGroup();
            _context.Groups.Add(group);
            _context.SaveChanges();

            var result = _repo.GetById(group.Id);

            Assert.NotNull(result);
            Assert.Equal(group.Id, result.Id);
        }

        [Fact]
        public void GetById_WhenNotExists_ReturnsNull()
        {
            var result = _repo.GetById(999);

            Assert.Null(result);
        }

        #endregion

        #region ADD TESTS

        [Fact]
        public void Add_ValidGroup_AddsToDatabase()
        {
            var group = CreateGroup();

            var result = _repo.Add(group);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Single(_context.Groups);
        }

        [Fact]
        public void Add_ValidGroup_PropertiesSavedCorrectly()
        {
            var group = CreateGroup("My Group");

            var result = _repo.Add(group);

            Assert.Equal("My Group", result.Name);
            Assert.Equal("Test School", result.School);
            Assert.False(result.IsLocked);
        }

        [Fact]
        public void Add_Group_CanBeRetrievedById()
        {
            var group = CreateGroup();

            var added = _repo.Add(group);
            var found = _repo.GetById(added.Id);

            Assert.NotNull(found);
            Assert.Equal(added.Id, found.Id);
        }

        #endregion

        #region DELETE TESTS

        [Fact]
        public void Delete_WhenNotExists_ReturnsNull()
        {
            var result = _repo.Delete(999);

            Assert.Null(result);
        }

        [Fact]
        public void Delete_WhenExists_RemovesGroup()
        {
            var group = _repo.Add(CreateGroup());

            var deleted = _repo.Delete(group.Id);
            var all = _repo.GetAll();

            Assert.NotNull(deleted);
            Assert.Empty(all);
        }

        [Fact]
        public void Delete_WhenExists_ReturnsDeletedGroup()
        {
            var group = _repo.Add(CreateGroup());

            var result = _repo.Delete(group.Id);

            Assert.NotNull(result);
            Assert.Equal(group.Id, result.Id);
        }

        [Fact]
        public void Delete_OneOfMultiple_RemovesOnlyOne()
        {
            var g1 = _repo.Add(CreateGroup("A"));
            var g2 = _repo.Add(CreateGroup("B"));

            _repo.Delete(g1.Id);
            var all = _repo.GetAll().ToList();

            Assert.Single(all);
            Assert.Equal(g2.Id, all[0].Id);
        }

        #endregion

        #region UPDATE TESTS

        [Fact]
        public void Update_WhenNotExists_ReturnsNull()
        {
            var updated = CreateGroup("Updated");

            var result = _repo.Update(999, updated);

            Assert.Null(result);
        }

        [Fact]
        public void Update_WhenExists_UpdatesGroup()
        {
            var group = _repo.Add(CreateGroup());

            var updated = new Group
            {
                Name = "Updated Name",
                School = "New School",
                IsLocked = true
            };

            var result = _repo.Update(group.Id, updated);

            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal("New School", result.School);
            Assert.True(result.IsLocked);
        }

        [Fact]
        public void Update_WhenExists_DoesNotChangeId()
        {
            var group = _repo.Add(CreateGroup());

            var updated = CreateGroup("New");
            updated.Id = 999;

            var result = _repo.Update(group.Id, updated);

            Assert.NotNull(result);
            Assert.Equal(group.Id, result.Id);
            Assert.NotEqual(999, result.Id);
        }

        [Fact]
        public void Update_OneOfMultiple_UpdatesOnlySelected()
        {
            var g1 = _repo.Add(CreateGroup("A"));
            var g2 = _repo.Add(CreateGroup("B"));

            var updated = new Group
            {
                Name = "Updated",
                School = "School",
                IsLocked = true
            };

            _repo.Update(g1.Id, updated);

            var first = _repo.GetById(g1.Id);
            var second = _repo.GetById(g2.Id);

            Assert.Equal("Updated", first.Name);
            Assert.Equal("B", second.Name);
        }

        #endregion
    }
}