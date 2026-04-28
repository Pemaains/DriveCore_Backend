using DriveCore.Dtos.Request;
using DriveCore.Dtos.Response;

namespace DriveCore.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<ServiceResult<CustomerResponse>> CreateCustomerAsync(CreateCustomerRequest request, string? createdByStaffId);
        Task<ServiceResult<CustomerResponse>> GetCustomerByIdAsync(int id);
        Task<ServiceResult<VehicleResponse>> AddVehicleAsync(int customerId, CreateVehicleRequest request);
    }
}
