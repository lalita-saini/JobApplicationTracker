using JobApplicationTracker.Config;
using JobApplicationTracker.Database;
using JobApplicationTracker.Exceptions;
using JobApplicationTracker.Models;
using JobApplicationTracker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using NUnit.Framework;
using Moq;

namespace JobApplicationTracker.Tests.Services
{
    [TestFixture]
    public class AuthServiceTests
    {
        private ApplicationDbContext _context;
        private AuthService _authService;
        private JwtSettings _jwtSettings;
        private readonly string _testEmail = "test@example.com";
        private readonly string _testPassword = "Test123!@#";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _jwtSettings = new JwtSettings
            {
                Secret = "your-test-secret-key-with-at-least-32-characters",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpirationInMinutes = 60
            };
        }

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();

            var jwtOptions = Options.Create(_jwtSettings);
            var logger = new Mock<ILogger<AuthService>>().Object;

            _authService = new AuthService(_context, jwtOptions, logger);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.CloseConnection();
            _context.Dispose();
        }

        [Test]
        public async Task RegisterAsync_WithValidRequest_ShouldCreateUser()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = _testEmail,
                Password = _testPassword,
                ConfirmPassword = _testPassword,
                FirstName = "Test",
                LastName = "User"
            };

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.Not.Null);
            Assert.That(result.Expiration, Is.GreaterThan(DateTime.UtcNow));

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == _testEmail);
            Assert.That(user, Is.Not.Null);
            Assert.That(user.FirstName, Is.EqualTo(request.FirstName));
            Assert.That(user.LastName, Is.EqualTo(request.LastName));
            Assert.That(user.PasswordHash, Is.Not.EqualTo(request.Password));
        }

        [Test]
        public async Task RegisterAsync_WithDuplicateEmail_ShouldThrowValidationException()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = _testEmail,
                Password = _testPassword,
                ConfirmPassword = _testPassword
            };

            await _authService.RegisterAsync(request);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _authService.RegisterAsync(request));

            Assert.That(ex.Errors, Does.ContainKey("Email"));
        }

        [Test]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = _testEmail,
                Password = _testPassword,
                ConfirmPassword = _testPassword
            };
            await _authService.RegisterAsync(registerRequest);

            var loginRequest = new LoginRequest
            {
                Email = _testEmail,
                Password = _testPassword
            };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.Not.Null);
            Assert.That(result.Expiration, Is.GreaterThan(DateTime.UtcNow));
        }

        [Test]
        public async Task LoginAsync_WithInvalidCredentials_ShouldThrowUnauthorizedException()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = _testEmail,
                Password = _testPassword,
                ConfirmPassword = _testPassword
            };
            await _authService.RegisterAsync(registerRequest);

            var loginRequest = new LoginRequest
            {
                Email = _testEmail,
                Password = "WrongPassword123!"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await _authService.LoginAsync(loginRequest));

            Assert.That(ex.Message, Is.EqualTo("Invalid email or password"));
        }
    }
}