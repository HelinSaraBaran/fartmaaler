using FartmaalerAPI.Controllers;
using FartmaalerAPI.Data;
using FartmaalerAPI.DTOs;
using FartmaalerAPI.Models;
using FartmaalerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FartmaalerAPI.Tests
{
    public class LeaderboardControllerTests
    {
        // Opretter fake in-memory database
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        // Opretter controller med rigtig service og fake database
        private LeaderboardController CreateController(AppDbContext context)
        {
            var service = new LeaderboardService(context);

            return new LeaderboardController(service);
        }

        [Fact]
        // Tester at admin class leaderboard returnerer BadRequest hvis vejtype mangler
        public void GetAdminClassLeaderboard_ReturnsBadRequest_WhenRoadTypeIsMissing()
        {
            using var context = GetDbContext();
            var controller = CreateController(context);

            var result = controller.GetAdminClassLeaderboard("");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        // Tester at admin class leaderboard returnerer Ok selvom listen er tom
        public void GetAdminClassLeaderboard_ReturnsOk_WhenNoDataExists()
        {
            using var context = GetDbContext();
            var controller = CreateController(context);

            var result = controller.GetAdminClassLeaderboard("byzone 50");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        // Tester at admin school leaderboard returnerer BadRequest hvis vejtype mangler
        public void GetAdminSchoolLeaderboard_ReturnsBadRequest_WhenRoadTypeIsMissing()
        {
            using var context = GetDbContext();
            var controller = CreateController(context);

            var result = controller.GetAdminSchoolLeaderboard("");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        // Tester at admin school leaderboard returnerer Ok selvom listen er tom
        public void GetAdminSchoolLeaderboard_ReturnsOk_WhenNoDataExists()
        {
            using var context = GetDbContext();
            var controller = CreateController(context);

            var result = controller.GetAdminSchoolLeaderboard("byzone 50");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        // Tester at student class leaderboard returnerer Forbid hvis leaderboard er slået fra
        public void GetStudentClassLeaderboard_ReturnsForbid_WhenLeaderboardIsDisabled()
        {
            using var context = GetDbContext();
            var controller = CreateController(context);

            var result = controller.GetStudentClassLeaderboard("byzone 50");

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        // Tester at student school leaderboard returnerer Forbid hvis leaderboard er slået fra
        public void GetStudentSchoolLeaderboard_ReturnsForbid_WhenLeaderboardIsDisabled()
        {
            using var context = GetDbContext();
            var controller = CreateController(context);

            var result = controller.GetStudentSchoolLeaderboard("byzone 50");

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        // Tester at student class leaderboard returnerer BadRequest hvis vejtype mangler og leaderboard er slået til
        public void GetStudentClassLeaderboard_ReturnsBadRequest_WhenRoadTypeIsMissing()
        {
            using var context = GetDbContext();

            context.Settings.Add(new Settings
            {
                Id = 1,
                Key = "Leaderboard",
                Value = true
            });

            context.SaveChanges();

            var controller = CreateController(context);

            var result = controller.GetStudentClassLeaderboard("");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        // Tester at student school leaderboard returnerer BadRequest hvis vejtype mangler og leaderboard er slået til
        public void GetStudentSchoolLeaderboard_ReturnsBadRequest_WhenRoadTypeIsMissing()
        {
            using var context = GetDbContext();

            context.Settings.Add(new Settings
            {
                Id = 1,
                Key = "Leaderboard",
                Value = true
            });

            context.SaveChanges();

            var controller = CreateController(context);

            var result = controller.GetStudentSchoolLeaderboard("");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        // Tester at GetLeaderboardSetting returnerer Ok
        public void GetLeaderboardSetting_ReturnsOk()
        {
            using var context = GetDbContext();
            var controller = CreateController(context);

            var result = controller.GetLeaderboardSetting();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        // Tester at UpdateLeaderboardSetting returnerer Ok
        public void UpdateLeaderboardSetting_ReturnsOk()
        {
            using var context = GetDbContext();
            var controller = CreateController(context);

            var request = new UpdateLeaderboardSettingRequest
            {
                IsEnabled = true
            };

            var result = controller.UpdateLeaderboardSetting(request);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        // Tester at UpdateLeaderboardSetting opretter/ændrer setting til true
        public void UpdateLeaderboardSetting_ChangesSettingValue()
        {
            using var context = GetDbContext();
            var controller = CreateController(context);

            var request = new UpdateLeaderboardSettingRequest
            {
                IsEnabled = true
            };

            var result = controller.UpdateLeaderboardSetting(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var setting = Assert.IsType<Settings>(okResult.Value);

            Assert.True(setting.Value);
        }
    }
}