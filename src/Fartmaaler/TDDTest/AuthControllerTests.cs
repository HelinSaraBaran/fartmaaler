using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BCrypt.Net;
using FartmaalerAPI.Controllers;
using FartmaalerAPI.Data;
using FartmaalerAPI.DTOs;
using FartmaalerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace TDDTest
{
    public class AuthControllerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AuthController _controller;
        private readonly IConfiguration _configuration;

        public AuthControllerTests()
        {
            // InMemory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            // Fake JWT settings
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Jwt:Key", "ThisIsASecretKeyForJwtTesting12345"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            _controller = new AuthController(_context, _configuration);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        private User CreateUser(
            string username = "admin",
            string password = "1234",
            string role = "Teacher")
        {
            User user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        [Fact]
        public void Login_WhenUserDoesNotExist_ReturnsUnauthorized()
        {
            // Arrange
            LoginRequest request = new LoginRequest
            {
                Username = "wronguser",
                Password = "1234"
            };

            // Act
            var result = _controller.Login(request);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void Login_WhenPasswordIsWrong_ReturnsUnauthorized()
        {
            // Arrange
            CreateUser();

            LoginRequest request = new LoginRequest
            {
                Username = "admin",
                Password = "wrongpassword"
            };

            // Act
            var result = _controller.Login(request);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void Login_WhenLoginIsCorrect_ReturnsOk()
        {
            // Arrange
            CreateUser();

            LoginRequest request = new LoginRequest
            {
                Username = "admin",
                Password = "1234"
            };

            // Act
            var result = _controller.Login(request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void Login_WhenLoginIsCorrect_ReturnsToken()
        {
            // Arrange
            CreateUser();

            LoginRequest request = new LoginRequest
            {
                Username = "admin",
                Password = "1234"
            };

            // Act
            var result = _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var response = Assert.IsType<LoginResponse>(okResult.Value);

            Assert.NotNull(response.Token);
            Assert.NotEmpty(response.Token);
        }

        [Fact]
        public void Login_WhenLoginIsCorrect_ReturnsCorrectUsername()
        {
            // Arrange
            CreateUser(username: "Julia");

            LoginRequest request = new LoginRequest
            {
                Username = "Julia",
                Password = "1234"
            };

            // Act
            var result = _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var response = Assert.IsType<LoginResponse>(okResult.Value);

            Assert.Equal("Julia", response.Username);
        }

        [Fact]
        public void Login_WhenLoginIsCorrect_ReturnsCorrectRole()
        {
            // Arrange
            CreateUser(role: "Admin");

            LoginRequest request = new LoginRequest
            {
                Username = "admin",
                Password = "1234"
            };

            // Act
            var result = _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var response = Assert.IsType<LoginResponse>(okResult.Value);

            Assert.Equal("Admin", response.Role);
        }

        [Fact]
        public void Login_WhenLoginIsCorrect_GeneratesValidJwtToken()
        {
            // Arrange
            CreateUser();

            LoginRequest request = new LoginRequest
            {
                Username = "admin",
                Password = "1234"
            };

            // Act
            var result = _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var response = Assert.IsType<LoginResponse>(okResult.Value);

            var handler = new JwtSecurityTokenHandler();

            var token = handler.ReadJwtToken(response.Token);

            Assert.Equal(
                "admin",
                token.Claims.First(c => c.Type == ClaimTypes.Name).Value);

            Assert.Equal(
                "Teacher",
                token.Claims.First(c => c.Type == ClaimTypes.Role).Value);

            Assert.NotNull(
                token.Claims.FirstOrDefault(c => c.Type == "userId"));
        }
    }
}