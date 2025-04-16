using AutoMapper;
using JobApplicationTracker.Exceptions;
using JobApplicationTracker.Middleware;
using JobApplicationTracker.Models;
using JobApplicationTracker.Models.Dtos;
using JobApplicationTracker.Respositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobApplicationTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JobApplicationsController : ControllerBase
    {
        private readonly IJobApplicationRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<JobApplicationsController> _logger;

        public JobApplicationsController(
            IJobApplicationRepository repository,
            IMapper mapper,
            ILogger<JobApplicationsController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedException("Invalid user token");
            }
            return userId;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<JobApplicationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<JobApplicationResponseDto>>> GetApplications()
        {
            var userId = GetCurrentUserId();

            var applications = await _repository.GetAllApplicationsAsync(userId);
            var responseDto = _mapper.Map<IEnumerable<JobApplicationResponseDto>>(applications);
            return Ok(responseDto);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(JobApplicationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<JobApplicationResponseDto>> GetApplication(int id)
        {
            var userId = GetCurrentUserId();
            var application = await _repository.GetApplicationByIdAsync(id, userId);
            if (application == null)
            {
                return NotFound(new ErrorResponse
                {
                    Message = $"Application with ID {id} not found.",
                    Errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    )
                });
            }
            var responseDto = _mapper.Map<JobApplicationResponseDto>(application);
            return Ok(responseDto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(JobApplicationResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateJobApplicationDto dto)
        {
            var userId = GetCurrentUserId();
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse 
                { 
                    Message = "Validation failed",
                    Errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    )
                });
            }

            var application = _mapper.Map<JobApplication>(dto);

            await _repository.AddApplicationAsync(application, userId);

            var responseDto = _mapper.Map<JobApplicationResponseDto>(application);
            return CreatedAtAction(
                nameof(GetApplication),
                new { id = application.Id },
                responseDto
            );
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateApplication(int id, [FromBody]UpdateJobApplicationDto dto)
        {
            var userId = GetCurrentUserId();
            var application = _mapper.Map<JobApplication>(dto);
            application.Id = id;

            await _repository.UpdateApplicationAsync(application, userId);
            return NoContent();
        }
    }
} 