using DriveCore.Dtos.Request;
using DriveCore.Dtos.Response;

namespace DriveCore.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<ServiceResult<CustomerResponse>> CreateCustomerAsync(CreateCustomerRequest request, string? createdByStaffId);
        Task<ServiceResult<CustomerResponse>> GetCustomerByIdAsync(int id);
        Task<ServiceResult<CustomerDetailResponse>> GetCustomerDetailAsync(int id);
        Task<ServiceResult<VehicleResponse>> AddVehicleAsync(int customerId, CreateVehicleRequest request);
        Task<ServiceResult<CustomerResponse>> GetCurrentCustomerAsync(string userId);
        Task<ServiceResult<CustomerResponse>> UpdateCurrentCustomerAsync(string userId, UpdateCustomerProfileRequest request);
        Task<ServiceResult<VehicleResponse>> AddCurrentCustomerVehicleAsync(string userId, CreateVehicleRequest request);
        Task<ServiceResult<VehicleResponse>> UpdateCurrentCustomerVehicleAsync(string userId, int vehicleId, UpdateVehicleRequest request);
        Task<ServiceResult<bool>> DeleteCurrentCustomerVehicleAsync(string userId, int vehicleId);
        Task<ServiceResult<CustomerHistoryResponse>> GetCurrentCustomerHistoryAsync(string userId);
    }
}
