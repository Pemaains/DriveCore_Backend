using DriveCore.Dtos.Request;
using DriveCore.Dtos.Response;
using DriveCore.Models;

namespace DriveCore.Services.Interfaces
{
    public interface IStaffService
    {
        Task<ServiceResult<StaffResponse>> CreateStaffAsync(CreateStaffRequest request);
        Task<List<StaffResponse>> GetAllStaffAsync();
        Task<ServiceResult<StaffResponse>> GetStaffByIdAsync(string id);
        Task<ServiceResult<StaffResponse>> UpdateStaffAsync(string id, UpdateStaffRequest request);
        Task<ServiceResult<StaffResponse>> UpdateStaffRoleAsync(string id, UserRole role);
        Task<ServiceResult<StaffResponse>> UpdateStaffStatusAsync(string id, bool isActive);
    }
}
