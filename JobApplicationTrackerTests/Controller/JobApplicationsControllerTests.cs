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
using JobApplicationTracker.Exceptions;
using System.Reflection;

namespace JobApplicationTrackerTests.Controller;

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
        var applications = new List<JobApplication> { new JobApplication { Id = 1 } };
        var dtos = new List<JobApplicationResponseDto> { new JobApplicationResponseDto { Id = 1 } };

        _mockRepository.Setup(r => r.GetAllApplicationsAsync(1)).ReturnsAsync(applications);
        _mockMapper.Setup(m => m.Map<IEnumerable<JobApplicationResponseDto>>(applications)).Returns(dtos);

        var result = await _controller.GetApplications();

        result.Result.Should().BeOfType<OkObjectResult>();
        (result.Result as OkObjectResult)!.Value.Should().BeEquivalentTo(dtos);
    }

    [Test]
    public async Task GetApplication_Existing_ReturnsOk()
    {
        var job = new JobApplication { Id = 99 };
        var dto = new JobApplicationResponseDto { Id = 99 };

        _mockRepository.Setup(r => r.GetApplicationByIdAsync(99, 1)).ReturnsAsync(job);
        _mockMapper.Setup(m => m.Map<JobApplicationResponseDto>(job)).Returns(dto);

        var result = await _controller.GetApplication(99);

        result.Result.Should().BeOfType<OkObjectResult>();
        (result.Result as OkObjectResult)!.Value.Should().BeEquivalentTo(dto);
    }

    [Test]
    public async Task GetApplication_NotFound_Returns404()
    {
        _mockRepository.Setup(r => r.GetApplicationByIdAsync(999, 1)).ReturnsAsync((JobApplication)null!);

        var result = await _controller.GetApplication(999);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task Create_WithValidData_ReturnsCreated()
    {
        var createDto = new CreateJobApplicationDto { CompanyName = "Test", Position = "Dev", Status = "Applied" };
        var app = new JobApplication();
        var responseDto = new JobApplicationResponseDto { Id = 10 };

        _mockMapper.Setup(m => m.Map<JobApplication>(createDto)).Returns(app);
        _mockRepository.Setup(r => r.AddApplicationAsync(app, 1)).Returns(Task.CompletedTask);
        _mockMapper.Setup(m => m.Map<JobApplicationResponseDto>(app)).Returns(responseDto);

        var result = await _controller.Create(createDto);

        result.Should().BeOfType<CreatedAtActionResult>();
        (result as CreatedAtActionResult)!.Value.Should().BeEquivalentTo(responseDto);
    }

    [Test]
    public async Task Create_InvalidModel_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("CompanyName", "Required");

        var dto = new CreateJobApplicationDto();

        var result = await _controller.Create(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task UpdateApplication_Valid_ReturnsNoContent()
    {
        var dto = new UpdateJobApplicationDto { CompanyName = "Updated", Position = "Lead Dev", Status = "Interview" };
        var mappedApp = new JobApplication { Id = 5 };

        _mockMapper.Setup(m => m.Map<JobApplication>(dto)).Returns(mappedApp);
        _mockRepository.Setup(r => r.UpdateApplicationAsync(mappedApp, 1)).Returns(Task.CompletedTask);

        var result = await _controller.UpdateApplication(5, dto);

        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public void GetCurrentUserId_InvalidClaim_ThrowsUnauthorized()
    {
        var controller = new JobApplicationsController(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        var act = () => controller.GetType()
            .GetMethod("GetCurrentUserId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(controller, null);

        act.Should().Throw<TargetInvocationException>()
            .WithInnerException<UnauthorizedException>()
            .WithMessage("Invalid user token");
    }
}
