using DriveCore.Dtos.Request;
using DriveCore.Dtos.Response;
using DriveCore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DriveCore.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (!result.Success)
            {
                return Unauthorized(new ErrorResponse
                {
                    Message = result.Message,
                    Errors = result.Errors
                });
            }

            return Ok(result.Data);
        }
    }
}
