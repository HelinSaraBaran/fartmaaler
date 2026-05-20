using FartmaalerAPI.Controllers;
using FartmaalerAPI.Data;
using FartmaalerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FartmaalerAPI.Tests
{
    public class SettingsControllerTests
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
        // Tester at GetAll returnerer Ok
        public void GetAll_ReturnsOk()
        {
            using var context = GetDbContext();

            var controller = new SettingsController(context);

            var result = controller.GetAll();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        // Tester at GetAll returnerer en liste med settings
        public void GetAll_ReturnsSettingsList()
        {
            using var context = GetDbContext();

            context.Settings.Add(new Settings
            {
                Id = 1,
                Key = "Leaderboard",
                Value = true
            });

            context.SaveChanges();

            var controller = new SettingsController(context);

            var result = controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var settings = Assert.IsType<List<Settings>>(okResult.Value);

            Assert.Single(settings);
        }

        [Fact]
        // Tester at Update returnerer NotFound hvis setting ikke findes
        public void Update_ReturnsNotFound_WhenSettingDoesNotExist()
        {
            using var context = GetDbContext();

            var controller = new SettingsController(context);

            var updatedSetting = new Settings
            {
                Key = "Leaderboard",
                Value = true
            };

            var result = controller.Update("Leaderboard", updatedSetting);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        // Tester at Update ændrer værdien på en eksisterende setting
        public void Update_ChangesExistingSetting()
        {
            using var context = GetDbContext();

            context.Settings.Add(new Settings
            {
                Id = 1,
                Key = "Leaderboard",
                Value = false
            });

            context.SaveChanges();

            var controller = new SettingsController(context);

            var updatedSetting = new Settings
            {
                Key = "Leaderboard",
                Value = true
            };

            var result = controller.Update("Leaderboard", updatedSetting);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var setting = Assert.IsType<Settings>(okResult.Value);

            Assert.True(setting.Value);
        }

        [Fact]
        // Tester at Update virker selvom key skrives med små bogstaver
        public void Update_Works_WhenKeyHasDifferentCasing()
        {
            using var context = GetDbContext();

            context.Settings.Add(new Settings
            {
                Id = 1,
                Key = "Leaderboard",
                Value = false
            });

            context.SaveChanges();

            var controller = new SettingsController(context);

            var updatedSetting = new Settings
            {
                Key = "leaderboard",
                Value = true
            };

            var result = controller.Update("leaderboard", updatedSetting);

            Assert.IsType<OkObjectResult>(result);
        }
    }
}