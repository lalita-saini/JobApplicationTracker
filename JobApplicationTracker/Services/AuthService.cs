using JobApplicationTracker.Config;
using JobApplicationTracker.Database;
using JobApplicationTracker.Exceptions;
using JobApplicationTracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BC = BCrypt.Net.BCrypt;

namespace JobApplicationTracker.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        string GenerateJwtToken(User user);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BC.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedException("Invalid email or password");
            }

            var token = GenerateJwtToken(user);
            
            return new AuthResponse
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes)
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Email", new[] { "Email already exists" } }
                });
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = BC.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes)
            };
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName ?? ""),
                new Claim(ClaimTypes.Surname, user.LastName ?? "")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
} 