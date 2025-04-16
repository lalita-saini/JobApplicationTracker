using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using JobApplicationTracker.Controllers;
using JobApplicationTracker.Services;
using JobApplicationTracker.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using JobApplicationTracker.Exceptions;
using JobApplicationTracker.Middleware;

namespace JobApplicationTracker.Tests.Controllers
{
    [TestFixture]
    public class AuthControllerTests
    {
        private Mock<IAuthService> _mockAuthService;
        private Mock<ILogger<AuthController>> _mockLogger;
        private AuthController _controller;

        [SetUp]
        public void Setup()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Register_WithValidRequest_ReturnsCreatedResult()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "Test",
                LastName = "User"
            };

            var authResponse = new AuthResponse
            {
                Token = "test-token",
                Expiration = DateTime.UtcNow.AddHours(1)
            };

            _mockAuthService.Setup(x => x.RegisterAsync(request))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Register(request);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Value.Should().BeEquivalentTo(authResponse);
            createdResult.ActionName.Should().Be(nameof(AuthController.Login));
        }

        [Test]
        public async Task Register_WhenEmailExists_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "existing@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            _mockAuthService.Setup(x => x.RegisterAsync(request))
                .ThrowsAsync(new ValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "Email", new[] { "Email already exists" } }
                    }));

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Value.Should().BeOfType<ErrorResponse>();
            var errorResponse = badRequestResult.Value as ErrorResponse;
            errorResponse.Errors["Email"].Should().Contain("Email already exists");
        }

        [Test]
        public async Task Login_WithValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var authResponse = new AuthResponse
            {
                Token = "test-token",
                Expiration = DateTime.UtcNow.AddHours(1)
            };

            _mockAuthService.Setup(x => x.LoginAsync(request))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(authResponse);
        }

        [Test]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            _mockAuthService.Setup(x => x.LoginAsync(request))
                .ThrowsAsync(new UnauthorizedException("Invalid email or password"));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var unauthorizedResult = result.Result as UnauthorizedObjectResult;
            unauthorizedResult.Value.Should().BeOfType<ErrorResponse>();
            var errorResponse = unauthorizedResult.Value as ErrorResponse;
            errorResponse.Message.Should().Be("Invalid email or password");
        }

        [Test]
        public async Task Register_WithPasswordMismatch_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "DifferentPassword123!"
            };

            _mockAuthService.Setup(x => x.RegisterAsync(request))
                .ThrowsAsync(new ValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "ConfirmPassword", new[] { "Passwords do not match" } }
                    }));

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Value.Should().BeOfType<ErrorResponse>();
            var errorResponse = badRequestResult.Value as ErrorResponse;
            errorResponse.Errors["ConfirmPassword"].Should().Contain("Passwords do not match");
        }
    }
}