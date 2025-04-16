using FluentAssertions;
using JobApplicationTracker.Models;
using Microsoft.Extensions.Logging;
using JobApplicationTracker.Exceptions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace JobApplicationTrackerTests.Repositories
{
    [TestFixture]
    public class JobApplicationRepositoryTests : DatabaseUtil
    {
        private JobApplicationRepository _repository;
        private Mock<ILogger<JobApplicationRepository>> _mockLogger;
        private User _testUser;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _mockLogger = new Mock<ILogger<JobApplicationRepository>>();
            _repository = new JobApplicationRepository(DbContext, _mockLogger.Object);
            _testUser = await CreateTestUser();
        }

        [Test]
        public async Task GetAllApplicationsAsync_ReturnsUserApplications()
        {
            // Arrange
            await CreateTestApplication(_testUser.Id, "Company 1");
            await CreateTestApplication(_testUser.Id, "Company 2");

            var otherUser = await CreateTestUser("other@example.com");
            await CreateTestApplication(otherUser.Id, "Other Company");

            // Act
            var result = await _repository.GetAllApplicationsAsync(_testUser.Id);

            // Assert
            result.Should().AllSatisfy(a => a.UserId.Should().Be(_testUser.Id));
        }

        [Test]
        public async Task AddApplicationAsync_AddsApplicationWithUserId()
        {
            // Arrange
            var application = new JobApplication
            {
                CompanyName = "New Company",
                Position = "Developer",
                Status = "Applied",
                DateApplied = DateTime.UtcNow
            };

            // Act
            await _repository.AddApplicationAsync(application, _testUser.Id);

            // Assert
            var savedApplication = await DbContext.Applications
                .FirstOrDefaultAsync(a => a.CompanyName == "New Company");
            savedApplication.Should().NotBeNull();
            savedApplication.UserId.Should().Be(_testUser.Id);
        }

        [Test]
        public async Task UpdateApplicationAsync_ThrowsNotFoundException_WhenApplicationNotFound()
        {
            // Arrange
            var application = new JobApplication
            {
                Id = 999,
                CompanyName = "Non-existent Company",
                UserId = _testUser.Id
            };

            // Act & Assert
            var act = () => _repository.UpdateApplicationAsync(application, _testUser.Id);
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task GetApplicationByIdAsync_ReturnsCorrectApplication()
        {
            // Arrange
            var otherUser = await CreateTestUser("other1@example.com");
            var application = await CreateTestApplication(otherUser.Id);

            // Act
            var result = await _repository.GetApplicationByIdAsync(application.Id, otherUser.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(application.Id);
            result.UserId.Should().Be(otherUser.Id);
        }
    }
}