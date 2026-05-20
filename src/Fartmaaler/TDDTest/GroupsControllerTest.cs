using FartmaalerAPI.Controllers;
using FartmaalerAPI.Data;
using FartmaalerAPI.DTOs;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Moq; 

namespace FartmaalerAPI.Tests
{
    public class GroupsControllerTests
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
        // Tester at GetAll returnerer grupper
        public void GetAll_ReturnsGroups()
        {
            using var context = GetDbContext();

            var groups = new List<Group>
            {
                new Group
                {
                    Id = 1,
                    Name = "Gruppe 1",
                    School = "Roskilde Skole"
                }
            };

            var repoMock = new Mock<IRepository<Group>>();

            repoMock.Setup(repo => repo.GetAll())
                .Returns(groups);

            var controller = new GroupsController(repoMock.Object, context);

            var result = controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            Assert.NotNull(okResult);
        }

        [Fact]
        // Tester at GetById returnerer NotFound hvis gruppen ikke findes
        public void GetById_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            using var context = GetDbContext();

            var repoMock = new Mock<IRepository<Group>>();

            repoMock.Setup(repo => repo.GetById(1))
                .Returns((Group?)null);

            var controller = new GroupsController(repoMock.Object, context);

            var result = controller.GetById(1);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        // Tester at GetById returnerer gruppe hvis den findes
        public void GetById_ReturnsGroup_WhenGroupExists()
        {
            using var context = GetDbContext();

            var group = new Group
            {
                Id = 1,
                Name = "Gruppe 1",
                School = "Roskilde Skole"
            };

            var repoMock = new Mock<IRepository<Group>>();

            repoMock.Setup(repo => repo.GetById(1))
                .Returns(group);

            var controller = new GroupsController(repoMock.Object, context);

            var result = controller.GetById(1);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        // Tester at Add returnerer BadRequest hvis request er null
        public void Add_ReturnsBadRequest_WhenRequestIsNull()
        {
            using var context = GetDbContext();

            var repoMock = new Mock<IRepository<Group>>();

            var controller = new GroupsController(repoMock.Object, context);

            var result = controller.Add(null);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        // Tester at Add returnerer BadRequest hvis navn er tomt
        public void Add_ReturnsBadRequest_WhenNameIsEmpty()
        {
            using var context = GetDbContext();

            var repoMock = new Mock<IRepository<Group>>();

            var controller = new GroupsController(repoMock.Object, context);

            var request = new CreateGroupRequest
            {
                Name = ""
            };

            var result = controller.Add(request);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        // Tester at Add opretter gruppe korrekt
        public void Add_CreatesGroup()
        {
            using var context = GetDbContext();

            var repoMock = new Mock<IRepository<Group>>();

            repoMock.Setup(repo => repo.Add(It.IsAny<Group>()))
                .Returns((Group group) =>
                {
                    group.Id = 1;
                    return group;
                });

            var controller = new GroupsController(repoMock.Object, context);

            var request = new CreateGroupRequest
            {
                Name = "Ny Gruppe"
            };

            var result = controller.Add(request);

            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        [Fact]
        // Tester at Add returnerer BadRequest hvis gruppen allerede findes
        public void Add_ReturnsBadRequest_WhenGroupAlreadyExists()
        {
            using var context = GetDbContext();

            context.Groups.Add(new Group
            {
                Id = 1,
                Name = "Gruppe 1",
                School = "Roskilde Skole"
            });

            context.SaveChanges();

            var repoMock = new Mock<IRepository<Group>>();

            var controller = new GroupsController(repoMock.Object, context);

            var request = new CreateGroupRequest
            {
                Name = "Gruppe 1"
            };

            var result = controller.Add(request);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        // Tester at Update returnerer NotFound hvis gruppen ikke findes
        public void Update_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            using var context = GetDbContext();

            var repoMock = new Mock<IRepository<Group>>();

            var controller = new GroupsController(repoMock.Object, context);

            var request = new UpdateGroupRequest
            {
                Name = "Test"
            };

            var result = controller.Update(1, request);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        // Tester at Update opdaterer gruppen korrekt
        public void Update_UpdatesGroup()
        {
            using var context = GetDbContext();

            context.Groups.Add(new Group
            {
                Id = 1,
                Name = "Gammel Gruppe",
                School = "Roskilde Skole"
            });

            context.SaveChanges();

            var repoMock = new Mock<IRepository<Group>>();

            var controller = new GroupsController(repoMock.Object, context);

            var request = new UpdateGroupRequest
            {
                Name = "Ny Gruppe"
            };

            var result = controller.Update(1, request);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        // Tester at Delete returnerer NotFound hvis gruppen ikke findes
        public void Delete_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            using var context = GetDbContext();

            var repoMock = new Mock<IRepository<Group>>();

            var controller = new GroupsController(repoMock.Object, context);

            var result = controller.Delete(1);

            Assert.IsType<NotFoundObjectResult>(result);
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
                School = "Roskilde Skole"
            });

            context.SaveChanges();

            var repoMock = new Mock<IRepository<Group>>();

            var controller = new GroupsController(repoMock.Object, context);

            var result = controller.Delete(1);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        // Tester at DeleteAllGroups returnerer Ok
        public void DeleteAllGroups_ReturnsOk()
        {
            using var context = GetDbContext();

            var repoMock = new Mock<IRepository<Group>>();

            var controller = new GroupsController(repoMock.Object, context);

            var result = controller.DeleteAllGroups();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        // Tester at GetGroupsOverview returnerer overview data
        public void GetGroupsOverview_ReturnsOverview()
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

            var repoMock = new Mock<IRepository<Group>>();

            var controller = new GroupsController(repoMock.Object, context);

            var result = controller.GetGroupsOverview();

            Assert.IsType<OkObjectResult>(result);
        }
    }
}