using FartmaalerAPI.Repositories;
using FartmaalerAPI.Models;   
using FartmaalerAPI;          

namespace TestProject1
{
    public class UnitTest1

    // TESTS FOR GROUPSREPO
    {
        [Fact]
        public void GetAll_ShouldReturnEmptyList_WhenRepoIsNew()
        {
            // Arrange
            GroupsRepo repo = new GroupsRepo();

            // Act
            IEnumerable<Group> groups = repo.GetAll();

            // Assert
            Assert.Empty(groups);
        }
        [Fact]
        public void Add_ShouldAddGroupToList()
        {
            GroupsRepo repo = new GroupsRepo();
            Group group = new Group { Name = "Test" };

            repo.Add(group);

            Assert.Single(repo.GetAll());
        }
        public void Add_ShouldReturnGroupWithId()
        {
            GroupsRepo repo = new GroupsRepo();
            Group group = new Group { Name = "Test" };

            Group result = repo.Add(group);

            Assert.Equal(1, result.Id);
        }
        [Fact]
        public void GetById_ShouldReturnGroup_WhenIdExists()
        {
            // Arrange
            GroupsRepo repo = new GroupsRepo();
            Group group = new Group { Name = "Test" };

            repo.Add(group);

            // Act
            Group? result = repo.GetById(group.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(group.Id, result.Id);
            Assert.Equal(group.Name, result.Name);
        }

        [Fact]
        public void GetById_ShouldReturnNull_WhenIdDoesNotExist()
        {
            GroupsRepo repo = new GroupsRepo();

            // Act
            Group? result = repo.GetById(999); // Non-existent ID

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Delete_ShouldRemoveGroup_WhenIdExists()
        {
            // Arrange
            GroupsRepo repo = new GroupsRepo();
            Group group = new Group { Name = "Test" };

            repo.Add(group);

            // Act
            Group? deleted = repo.Delete(group.Id);

            // Assert
            Assert.NotNull(deleted);
            Assert.Empty(repo.GetAll());
        }
        [Fact]
        public void Delete_ShouldReturnDeletedGroup()
        {
            GroupsRepo repo = new GroupsRepo();
            Group group = new Group { Name = "Test" };

            repo.Add(group);

            Group? deleted = repo.Delete(group.Id);

            Assert.Equal(group.Id, deleted.Id);
            Assert.Equal(group.Name, deleted.Name);
        }
        [Fact]
        public void Delete_ShouldReturnNull_WhenIdDoesNotExist()
        {
            GroupsRepo repo = new GroupsRepo();

            Group? result = repo.Delete(999);

            Assert.Null(result);
        }
        [Fact]
        public void Delete_ShouldRemoveOnlyOneGroup()
        {
            GroupsRepo repo = new GroupsRepo();
            Group g1 = new Group { Name = "A" };
            Group g2 = new Group { Name = "B" };

            repo.Add(g1);
            repo.Add(g2);

            repo.Delete(g1.Id);

            Assert.Single(repo.GetAll());
        }
        [Fact]
        public void Update_ShouldUpdateGroup_WhenIdExists()
        {
            // Arrange
            GroupsRepo repo = new GroupsRepo();

            Group group = new Group
            {
                Name = "Old name",
                School = "Old school",
                IsLocked = false
            };

            repo.Add(group);

            Group updatedGroup = new Group
            {
                Name = "New name",
                School = "New school",
                IsLocked = true
            };

            // Act
            Group? result = repo.Update(group.Id, updatedGroup);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(group.Id, result.Id);
            Assert.Equal("New name", result.Name);
            Assert.Equal("New school", result.School);
            Assert.True(result.IsLocked);
        }
        [Fact]
        public void Update_ShouldReturnNull_WhenIdDoesNotExist()
        {
            // Arrange
            GroupsRepo repo = new GroupsRepo();

            Group updatedGroup = new Group
            {
                Name = "New name",
                School = "New school",
                IsLocked = true
            };

            // Act
            Group? result = repo.Update(999, updatedGroup);

            // Assert
            Assert.Null(result);
        }
        [Fact]
        public void Update_ShouldChangeGroupInRepo()
        {
            // Arrange
            GroupsRepo repo = new GroupsRepo();

            Group group = new Group
            {
                Name = "Old name",
                School = "Old school",
                IsLocked = false
            };

            repo.Add(group);

            Group updatedGroup = new Group
            {
                Name = "New name",
                School = "New school",
                IsLocked = true
            };

            // Act
            repo.Update(group.Id, updatedGroup);
            Group? result = repo.GetById(group.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New name", result.Name);
            Assert.Equal("New school", result.School);
            Assert.True(result.IsLocked);
        }

    }
}