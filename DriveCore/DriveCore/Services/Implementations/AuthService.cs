using DriveCore.Dtos.Request;
using DriveCore.Dtos.Response;
using DriveCore.Models;
using DriveCore.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DriveCore.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private const int DefaultExpiryMinutes = 240;

        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
        {
            var email = request.Email.Trim();
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return ServiceResult<AuthResponse>.Fail("Invalid email or password");
            }

            if (!user.IsActive)
            {
                return ServiceResult<AuthResponse>.Fail("Account is inactive");
            }

            return ServiceResult<AuthResponse>.Ok(new AuthResponse
            {
                Token = CreateToken(user),
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = user.Role
            });
        }

        private string CreateToken(ApplicationUser user)
        {
            var email = user.Email ?? string.Empty;
            var role = user.Role.ToString();

            // Standard claims power ASP.NET authorization; custom claims keep the token easy to read.
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Role, role),
                new("UserId", user.Id),
                new("FullName", user.FullName),
                new("Email", email),
                new("Role", role)
            };

            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var expiryMinutes = GetExpiryMinutes();

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int GetExpiryMinutes()
        {
            return int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var expiryMinutes) && expiryMinutes > 0
                ? expiryMinutes
                : DefaultExpiryMinutes;
        }
    }
}
