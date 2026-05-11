using DriveCore.Dtos.Request;
using DriveCore.Dtos.Response;

namespace DriveCore.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request);
        Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request);
    }
}
