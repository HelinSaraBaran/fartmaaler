using FartmaalerAPI.Controllers;
using FartmaalerAPI.Data;
using FartmaalerAPI.DTOs;
using FartmaalerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace FartmaalerAPI.Tests
{
    public class AuthControllerTests
    {
        // Opretter en fake in-memory database til hver test
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        // Opretter fake JWT settings, så controlleren kan lave token
        private IConfiguration GetConfiguration()
        {
            var settings = new Dictionary<string, string?>
            {
                { "Jwt:Key", "ThisIsASecretTestKeyForJwtThatIsLongEnough12345" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        [Fact]
        // Tester at login returnerer Unauthorized, hvis brugeren ikke findes
        public void Login_ReturnsUnauthorized_WhenUserDoesNotExist()
        {
            using var context = GetDbContext();
            var configuration = GetConfiguration();
            var controller = new AuthController(context, configuration);

            var request = new LoginRequest
            {
                Username = "admin",
                Password = "1234"
            };

            var result = controller.Login(request);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        // Tester at login returnerer Unauthorized, hvis password er forkert
        public void Login_ReturnsUnauthorized_WhenPasswordIsWrong()
        {
            using var context = GetDbContext();

            context.Users.Add(new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctPassword"),
                Role = "Teacher"
            });

            context.SaveChanges();

            var configuration = GetConfiguration();
            var controller = new AuthController(context, configuration);

            var request = new LoginRequest
            {
                Username = "admin",
                Password = "wrongPassword"
            };

            var result = controller.Login(request);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        // Tester at login returnerer Ok, hvis brugernavn og password er korrekte
        public void Login_ReturnsOk_WhenLoginIsCorrect()
        {
            using var context = GetDbContext();

            context.Users.Add(new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
                Role = "Teacher"
            });

            context.SaveChanges();

            var configuration = GetConfiguration();
            var controller = new AuthController(context, configuration);

            var request = new LoginRequest
            {
                Username = "admin",
                Password = "1234"
            };

            var result = controller.Login(request);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        // Tester at login response indeholder token, brugernavn og rolle
        public void Login_ReturnsLoginResponse_WhenLoginIsCorrect()
        {
            using var context = GetDbContext();

            context.Users.Add(new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
                Role = "Teacher"
            });

            context.SaveChanges();

            var configuration = GetConfiguration();
            var controller = new AuthController(context, configuration);

            var request = new LoginRequest
            {
                Username = "admin",
                Password = "1234"
            };

            var result = controller.Login(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LoginResponse>(okResult.Value);

            Assert.False(string.IsNullOrWhiteSpace(response.Token));
            Assert.Equal("admin", response.Username);
            Assert.Equal("Teacher", response.Role);
        }
    }
}