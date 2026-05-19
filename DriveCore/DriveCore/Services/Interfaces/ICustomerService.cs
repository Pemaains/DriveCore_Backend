using DriveCore.Dtos.Request;
using DriveCore.Dtos.Response;

namespace DriveCore.Services.Interfaces
{
    /// <summary>
    /// Defines customer profile, vehicle, and customer self-service operations.
    /// </summary>
    public interface ICustomerService
    {
        #region Staff Customer Management

        /// <summary>
        /// Creates a new customer account and profile.
        /// </summary>
        Task<ServiceResult<CustomerResponse>> CreateCustomerAsync(CreateCustomerRequest request, string? createdByStaffId);

        /// <summary>
        /// Retrieves a customer by profile identifier.
        /// </summary>
        Task<ServiceResult<CustomerResponse>> GetCustomerByIdAsync(int id);

        /// <summary>
        /// Retrieves a detailed customer record including invoice summaries.
        /// </summary>
        Task<ServiceResult<CustomerDetailResponse>> GetCustomerDetailAsync(int id);

        /// <summary>
        /// Adds a vehicle to an existing customer profile.
        /// </summary>
        Task<ServiceResult<VehicleResponse>> AddVehicleAsync(int customerId, CreateVehicleRequest request);

        #endregion

        #region Customer Self-Service

        /// <summary>
        /// Retrieves the current authenticated customer's profile.
        /// </summary>
        Task<ServiceResult<CustomerResponse>> GetCurrentCustomerAsync(string userId);

        /// <summary>
        /// Updates the current authenticated customer's profile.
        /// </summary>
        Task<ServiceResult<CustomerResponse>> UpdateCurrentCustomerAsync(string userId, UpdateCustomerProfileRequest request);

        /// <summary>
        /// Adds a vehicle for the current authenticated customer.
        /// </summary>
        Task<ServiceResult<VehicleResponse>> AddCurrentCustomerVehicleAsync(string userId, CreateVehicleRequest request);

        /// <summary>
        /// Updates a vehicle belonging to the current authenticated customer.
        /// </summary>
        Task<ServiceResult<VehicleResponse>> UpdateCurrentCustomerVehicleAsync(string userId, int vehicleId, UpdateVehicleRequest request);

        /// <summary>
        /// Deletes a vehicle belonging to the current authenticated customer.
        /// </summary>
        Task<ServiceResult<bool>> DeleteCurrentCustomerVehicleAsync(string userId, int vehicleId);

        /// <summary>
        /// Retrieves purchase and service history for the current authenticated customer.
        /// </summary>
        Task<ServiceResult<CustomerHistoryResponse>> GetCurrentCustomerHistoryAsync(string userId);

        #endregion
    }
}
