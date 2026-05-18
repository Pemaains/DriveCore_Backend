using DriveCore.Data;
using DriveCore.Dtos.Request;
using DriveCore.Dtos.Response;
using DriveCore.Models;
using DriveCore.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DriveCore.Services.Implementations
{
    /// <summary>
    /// Provides customer profile, vehicle, and history operations for the API.
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="userManager">The ASP.NET Identity user manager.</param>
        public CustomerService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        #region Staff Customer Management

        /// <inheritdoc />
        public async Task<ServiceResult<CustomerResponse>> CreateCustomerAsync(CreateCustomerRequest request, string? createdByStaffId)
        {
            var emailExists = await _userManager.FindByEmailAsync(request.Email);
            if (emailExists is not null)
            {
                return ServiceResult<CustomerResponse>.Fail("Email address is already registered.");
            }

            var duplicateVehicleNumbers = request.Vehicles
                .GroupBy(vehicle => vehicle.VehicleNumber.Trim().ToUpperInvariant())
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (duplicateVehicleNumbers.Any())
            {
                return ServiceResult<CustomerResponse>.Fail("Duplicate vehicle numbers were provided.", duplicateVehicleNumbers);
            }

            var existingVehicleNumbers = request.Vehicles.Select(vehicle => vehicle.VehicleNumber.Trim()).ToList();
            var vehicleExists = await _context.Vehicles
                .AnyAsync(vehicle => existingVehicleNumbers.Contains(vehicle.VehicleNumber));

            if (vehicleExists)
            {
                return ServiceResult<CustomerResponse>.Fail("One or more vehicle numbers are already registered.");
            }

            var user = new ApplicationUser
            {
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim(),
                UserName = request.Email.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                return ServiceResult<CustomerResponse>.Fail(
                    "Could not create customer account.",
                    createResult.Errors.Select(error => error.Description));
            }

            await _userManager.AddToRoleAsync(user, UserRole.Customer.ToString());

            var customerProfile = new CustomerProfile
            {
                UserId = user.Id,
                Address = request.Address.Trim(),
                CreatedByStaffId = createdByStaffId,
                Vehicles = request.Vehicles.Select(vehicle => new Vehicle
                {
                    VehicleNumber = vehicle.VehicleNumber.Trim(),
                    Brand = vehicle.Brand.Trim(),
                    Model = vehicle.Model.Trim(),
                    Year = vehicle.Year,
                    Color = vehicle.Color?.Trim()
                }).ToList()
            };

            _context.CustomerProfiles.Add(customerProfile);
            await _context.SaveChangesAsync();

            customerProfile.User = user;
            return ServiceResult<CustomerResponse>.Ok(MapCustomer(customerProfile), "Customer registered successfully.");
        }

        /// <inheritdoc />
        public async Task<ServiceResult<CustomerResponse>> GetCustomerByIdAsync(int id)
        {
            var customer = await _context.CustomerProfiles
                .Include(profile => profile.User)
                .Include(profile => profile.Vehicles)
                .FirstOrDefaultAsync(profile => profile.Id == id);

            return customer is null
                ? ServiceResult<CustomerResponse>.Fail("Customer was not found.")
                : ServiceResult<CustomerResponse>.Ok(MapCustomer(customer));
        }

        /// <inheritdoc />
        public async Task<ServiceResult<CustomerDetailResponse>> GetCustomerDetailAsync(int id)
        {
            var customer = await _context.CustomerProfiles
                .Include(profile => profile.User)
                .Include(profile => profile.Vehicles)
                .Include(profile => profile.SalesInvoices)
                .FirstOrDefaultAsync(profile => profile.Id == id);

            return customer is null
                ? ServiceResult<CustomerDetailResponse>.Fail("Customer was not found.")
                : ServiceResult<CustomerDetailResponse>.Ok(MapCustomerDetail(customer));
        }

        /// <inheritdoc />
        public async Task<ServiceResult<VehicleResponse>> AddVehicleAsync(int customerId, CreateVehicleRequest request)
        {
            var customerExists = await _context.CustomerProfiles.AnyAsync(customer => customer.Id == customerId);
            if (!customerExists)
            {
                return ServiceResult<VehicleResponse>.Fail("Customer was not found.");
            }

            var vehicleExists = await _context.Vehicles
                .AnyAsync(vehicle => vehicle.VehicleNumber == request.VehicleNumber.Trim());

            if (vehicleExists)
            {
                return ServiceResult<VehicleResponse>.Fail("Vehicle number is already registered.");
            }

            var vehicle = new Vehicle
            {
                CustomerProfileId = customerId,
                VehicleNumber = request.VehicleNumber.Trim(),
                Brand = request.Brand.Trim(),
                Model = request.Model.Trim(),
                Year = request.Year,
                Color = request.Color?.Trim()
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return ServiceResult<VehicleResponse>.Ok(MapVehicle(vehicle), "Vehicle added successfully.");
        }

        #endregion

        #region Customer Self-Service

        /// <inheritdoc />
        public async Task<ServiceResult<CustomerResponse>> GetCurrentCustomerAsync(string userId)
        {
            var customer = await FindCustomerByUserIdAsync(userId);

            return customer is null
                ? ServiceResult<CustomerResponse>.Fail("Customer profile was not found.")
                : ServiceResult<CustomerResponse>.Ok(MapCustomer(customer));
        }

        /// <inheritdoc />
        public async Task<ServiceResult<CustomerResponse>> UpdateCurrentCustomerAsync(string userId, UpdateCustomerProfileRequest request)
        {
            var customer = await FindCustomerByUserIdAsync(userId);
            if (customer is null)
            {
                return ServiceResult<CustomerResponse>.Fail("Customer profile was not found.");
            }

            customer.User.FullName = request.FullName.Trim();
            customer.User.PhoneNumber = request.PhoneNumber.Trim();
            customer.Address = request.Address.Trim();

            await _context.SaveChangesAsync();

            return ServiceResult<CustomerResponse>.Ok(MapCustomer(customer), "Profile updated successfully.");
        }

        /// <inheritdoc />
        public async Task<ServiceResult<VehicleResponse>> AddCurrentCustomerVehicleAsync(string userId, CreateVehicleRequest request)
        {
            var customer = await FindCustomerByUserIdAsync(userId);
            if (customer is null)
            {
                return ServiceResult<VehicleResponse>.Fail("Customer profile was not found.");
            }

            var vehicleNumber = request.VehicleNumber.Trim();
            var vehicleExists = await _context.Vehicles
                .AnyAsync(vehicle => vehicle.VehicleNumber == vehicleNumber);

            if (vehicleExists)
            {
                return ServiceResult<VehicleResponse>.Fail("Vehicle number is already registered.");
            }

            var vehicle = new Vehicle
            {
                CustomerProfileId = customer.Id,
                VehicleNumber = vehicleNumber,
                Brand = request.Brand.Trim(),
                Model = request.Model.Trim(),
                Year = request.Year,
                Color = request.Color?.Trim()
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return ServiceResult<VehicleResponse>.Ok(MapVehicle(vehicle), "Vehicle added successfully.");
        }

        /// <inheritdoc />
        public async Task<ServiceResult<VehicleResponse>> UpdateCurrentCustomerVehicleAsync(string userId, int vehicleId, UpdateVehicleRequest request)
        {
            var customer = await FindCustomerByUserIdAsync(userId);
            if (customer is null)
            {
                return ServiceResult<VehicleResponse>.Fail("Customer profile was not found.");
            }

            var vehicle = customer.Vehicles.FirstOrDefault(item => item.Id == vehicleId);
            if (vehicle is null)
            {
                return ServiceResult<VehicleResponse>.Fail("Vehicle was not found.");
            }

            var vehicleNumber = request.VehicleNumber.Trim();
            var vehicleExists = await _context.Vehicles
                .AnyAsync(item => item.Id != vehicleId && item.VehicleNumber == vehicleNumber);

            if (vehicleExists)
            {
                return ServiceResult<VehicleResponse>.Fail("Vehicle number is already registered.");
            }

            vehicle.VehicleNumber = vehicleNumber;
            vehicle.Brand = request.Brand.Trim();
            vehicle.Model = request.Model.Trim();
            vehicle.Year = request.Year;
            vehicle.Color = request.Color?.Trim();

            await _context.SaveChangesAsync();

            return ServiceResult<VehicleResponse>.Ok(MapVehicle(vehicle), "Vehicle updated successfully.");
        }

        /// <inheritdoc />
        public async Task<ServiceResult<bool>> DeleteCurrentCustomerVehicleAsync(string userId, int vehicleId)
        {
            var customer = await FindCustomerByUserIdAsync(userId);
            if (customer is null)
            {
                return ServiceResult<bool>.Fail("Customer profile was not found.");
            }

            var vehicle = customer.Vehicles.FirstOrDefault(item => item.Id == vehicleId);
            if (vehicle is null)
            {
                return ServiceResult<bool>.Fail("Vehicle was not found.");
            }

            if (customer.Vehicles.Count <= 1)
            {
                return ServiceResult<bool>.Fail("At least one vehicle is required.");
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true, "Vehicle deleted successfully.");
        }

        /// <inheritdoc />
        public async Task<ServiceResult<CustomerHistoryResponse>> GetCurrentCustomerHistoryAsync(string userId)
        {
            var customer = await _context.CustomerProfiles
                .Include(profile => profile.User)
                .Include(profile => profile.SalesInvoices)
                    .ThenInclude(invoice => invoice.Vehicle)
                .Include(profile => profile.SalesInvoices)
                    .ThenInclude(invoice => invoice.Items)
                        .ThenInclude(item => item.Part)
                .FirstOrDefaultAsync(profile => profile.UserId == userId);

            if (customer is null)
            {
                return ServiceResult<CustomerHistoryResponse>.Fail("Customer profile was not found.");
            }

            var appointmentCustomerKeys = new[] { customer.UserId, customer.Id.ToString() };
            var appointments = await _context.Appointments
                .Where(appointment => appointmentCustomerKeys.Contains(appointment.CustomerId))
                .OrderByDescending(appointment => appointment.AppointmentDate)
                .ToListAsync();

            return ServiceResult<CustomerHistoryResponse>.Ok(new CustomerHistoryResponse
            {
                CustomerProfileId = customer.Id,
                FullName = customer.User.FullName,
                GeneratedAt = DateTime.UtcNow,
                TotalPurchaseCount = customer.SalesInvoices.Count,
                TotalPurchaseAmount = customer.SalesInvoices.Sum(invoice => invoice.TotalAmount),
                TotalServiceCount = appointments.Count,
                Purchases = customer.SalesInvoices
                    .OrderByDescending(invoice => invoice.CreatedAt)
                    .Select(invoice => new PurchaseHistoryItemResponse
                    {
                        Id = invoice.Id,
                        InvoiceNumber = invoice.InvoiceNumber,
                        CreatedAt = invoice.CreatedAt,
                        TotalAmount = invoice.TotalAmount,
                        VehicleId = invoice.VehicleId,
                        VehicleNumber = invoice.Vehicle?.VehicleNumber,
                        Items = invoice.Items
                            .OrderBy(item => item.Part.Name)
                            .Select(item => new PurchaseHistoryLineItemResponse
                            {
                                PartId = item.PartId,
                                PartName = item.Part.Name,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice,
                                LineTotal = item.LineTotal
                            })
                            .ToList()
                    })
                    .ToList(),
                Services = appointments
                    .Select(appointment => new ServiceHistoryItemResponse
                    {
                        Id = appointment.Id,
                        AppointmentDate = appointment.AppointmentDate,
                        ServiceType = appointment.ServiceType,
                        Status = appointment.Status,
                        Notes = appointment.Notes,
                        CreatedAt = appointment.CreatedAt
                    })
                    .ToList()
            });
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Finds a customer profile by the owning application user identifier.
        /// </summary>
        private async Task<CustomerProfile?> FindCustomerByUserIdAsync(string userId)
        {
            return await _context.CustomerProfiles
                .Include(profile => profile.User)
                .Include(profile => profile.Vehicles)
                .FirstOrDefaultAsync(profile => profile.UserId == userId);
        }

        /// <summary>
        /// Maps a customer profile to a lightweight customer response.
        /// </summary>
        private static CustomerResponse MapCustomer(CustomerProfile customer)
        {
            return new CustomerResponse
            {
                UserId = customer.UserId,
                CustomerProfileId = customer.Id,
                FullName = customer.User.FullName,
                Email = customer.User.Email ?? string.Empty,
                PhoneNumber = customer.User.PhoneNumber,
                Address = customer.Address,
                CreatedByStaffId = customer.CreatedByStaffId,
                Vehicles = customer.Vehicles.Select(MapVehicle).ToList()
            };
        }

        /// <summary>
        /// Maps a customer profile to a detailed response including invoice summaries.
        /// </summary>
        private static CustomerDetailResponse MapCustomerDetail(CustomerProfile customer)
        {
            return new CustomerDetailResponse
            {
                UserId = customer.UserId,
                CustomerProfileId = customer.Id,
                FullName = customer.User.FullName,
                Email = customer.User.Email ?? string.Empty,
                PhoneNumber = customer.User.PhoneNumber,
                Address = customer.Address,
                CreatedByStaffId = customer.CreatedByStaffId,
                Vehicles = customer.Vehicles.Select(MapVehicle).ToList(),
                Invoices = customer.SalesInvoices
                    .OrderByDescending(invoice => invoice.CreatedAt)
                    .Select(MapInvoiceSummary)
                    .ToList()
            };
        }

        /// <summary>
        /// Maps an invoice to a summary item used in customer detail responses.
        /// </summary>
        private static SalesInvoiceSummaryResponse MapInvoiceSummary(SalesInvoice invoice)
        {
            return new SalesInvoiceSummaryResponse
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CreatedAt = invoice.CreatedAt,
                TotalAmount = invoice.TotalAmount,
                VehicleId = invoice.VehicleId
            };
        }

        /// <summary>
        /// Maps a vehicle entity to its response model.
        /// </summary>
        private static VehicleResponse MapVehicle(Vehicle vehicle)
        {
            return new VehicleResponse
            {
                Id = vehicle.Id,
                VehicleNumber = vehicle.VehicleNumber,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Color = vehicle.Color
            };
        }

        #endregion
    }
}
