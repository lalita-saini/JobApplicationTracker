using JobApplicationTracker.Exceptions;
using JobApplicationTracker.Middleware;
using JobApplicationTracker.Models;
using JobApplicationTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobApplicationTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService)); ;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); ;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(new ErrorResponse
                {
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt for user {Email}", request.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Message = "An error occurred while processing your request."
                });
            }
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {

                var response = await _authService.RegisterAsync(request);
                return CreatedAtAction(nameof(Login), response);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Message = ex.Message,
                    Errors = ex.Errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Email}", request.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Message = "An error occurred while processing your request."
                });
            }
        }
    }
} 