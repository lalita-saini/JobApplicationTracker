using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using JobApplicationTracker.Respositories;
using AutoMapper;
using JobApplicationTracker.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using JobApplicationTracker.Models;
using JobApplicationTracker.Models.Dtos;

namespace JobApplicationTrackerTests.Controller
{
    [TestFixture]
    public class JobApplicationsControllerTests
    {
        private Mock<IJobApplicationRepository> _mockRepository;
        private Mock<IMapper> _mockMapper;
        private Mock<ILogger<JobApplicationsController>> _mockLogger;
        private JobApplicationsController _controller;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<IJobApplicationRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<JobApplicationsController>>();

            // Setup controller with mock user
            _controller = new JobApplicationsController(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task GetApplications_ReturnsOkResult_WithApplications()
        {
            // Arrange
            var applications = new List<JobApplication>
            {
                new JobApplication { Id = 1, CompanyName = "Test Company" }
            };
            var dtos = new List<JobApplicationResponseDto>
            {
                new JobApplicationResponseDto { Id = 1, CompanyName = "Test Company" }
            };

            _mockRepository.Setup(repo => repo.GetAllApplicationsAsync(1))
                .ReturnsAsync(applications);
            _mockMapper.Setup(m => m.Map<IEnumerable<JobApplicationResponseDto>>(applications))
                .Returns(dtos);

            // Act
            var result = await _controller.GetApplications();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(dtos);
        }

        [Test]
        public async Task Create_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var createDto = new CreateJobApplicationDto
            {
                CompanyName = "Test Company",
                Position = "Developer",
                Status = "Applied"
            };
            var application = new JobApplication();
            var responseDto = new JobApplicationResponseDto { Id = 1 };

            _mockMapper.Setup(m => m.Map<JobApplication>(createDto))
                .Returns(application);
            _mockMapper.Setup(m => m.Map<JobApplicationResponseDto>(application))
                .Returns(responseDto);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult.Value.Should().BeEquivalentTo(responseDto);
        }
    }
}