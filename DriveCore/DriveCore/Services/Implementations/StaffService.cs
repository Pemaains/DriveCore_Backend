using DriveCore.Data;
using DriveCore.Dtos.Request;
using DriveCore.Dtos.Response;
using DriveCore.Models;
using DriveCore.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DriveCore.Services.Implementations
{
    public class StaffService : IStaffService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StaffService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ServiceResult<StaffResponse>> CreateStaffAsync(CreateStaffRequest request)
        {
            if (request.Role == UserRole.Customer)
            {
                return ServiceResult<StaffResponse>.Fail("Staff role cannot be Customer.");
            }

            var emailExists = await _userManager.FindByEmailAsync(request.Email);
            if (emailExists is not null)
            {
                return ServiceResult<StaffResponse>.Fail("Email address is already registered.");
            }

            var staffCodeExists = await _context.StaffProfiles
                .AnyAsync(staff => staff.StaffCode == request.StaffCode);

            if (staffCodeExists)
            {
                return ServiceResult<StaffResponse>.Fail("Staff code is already in use.");
            }

            var user = new ApplicationUser
            {
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim(),
                UserName = request.Email.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                return ServiceResult<StaffResponse>.Fail(
                    "Could not create staff account.",
                    createResult.Errors.Select(error => error.Description));
            }

            await _userManager.AddToRoleAsync(user, request.Role.ToString());

            var staffProfile = new StaffProfile
            {
                UserId = user.Id,
                StaffCode = request.StaffCode.Trim(),
                Position = request.Position.Trim()
            };

            _context.StaffProfiles.Add(staffProfile);
            await _context.SaveChangesAsync();

            staffProfile.User = user;
            return ServiceResult<StaffResponse>.Ok(MapStaff(staffProfile), "Staff account created successfully.");
        }

        public async Task<List<StaffResponse>> GetAllStaffAsync()
        {
            return await _context.StaffProfiles
                .Include(staff => staff.User)
                .OrderBy(staff => staff.User.FullName)
                .Select(staff => MapStaff(staff))
                .ToListAsync();
        }

        public async Task<ServiceResult<StaffResponse>> GetStaffByIdAsync(string id)
        {
            var staff = await FindStaffAsync(id);
            return staff is null
                ? ServiceResult<StaffResponse>.Fail("Staff account was not found.")
                : ServiceResult<StaffResponse>.Ok(MapStaff(staff));
        }

        public async Task<ServiceResult<StaffResponse>> UpdateStaffAsync(string id, UpdateStaffRequest request)
        {
            var staff = await FindStaffAsync(id);
            if (staff is null)
            {
                return ServiceResult<StaffResponse>.Fail("Staff account was not found.");
            }

            staff.User.FullName = request.FullName.Trim();
            staff.User.PhoneNumber = request.PhoneNumber.Trim();
            staff.Position = request.Position.Trim();

            await _context.SaveChangesAsync();
            return ServiceResult<StaffResponse>.Ok(MapStaff(staff), "Staff details updated successfully.");
        }

        public async Task<ServiceResult<StaffResponse>> UpdateStaffRoleAsync(string id, UserRole role)
        {
            if (role == UserRole.Customer)
            {
                return ServiceResult<StaffResponse>.Fail("Staff account cannot be assigned the Customer role.");
            }

            var staff = await FindStaffAsync(id);
            if (staff is null)
            {
                return ServiceResult<StaffResponse>.Fail("Staff account was not found.");
            }

            var currentRoles = await _userManager.GetRolesAsync(staff.User);
            await _userManager.RemoveFromRolesAsync(staff.User, currentRoles);
            await _userManager.AddToRoleAsync(staff.User, role.ToString());

            staff.User.Role = role;
            await _context.SaveChangesAsync();

            return ServiceResult<StaffResponse>.Ok(MapStaff(staff), "Staff role updated successfully.");
        }

        public async Task<ServiceResult<StaffResponse>> UpdateStaffStatusAsync(string id, bool isActive)
        {
            var staff = await FindStaffAsync(id);
            if (staff is null)
            {
                return ServiceResult<StaffResponse>.Fail("Staff account was not found.");
            }

            staff.User.IsActive = isActive;
            await _context.SaveChangesAsync();

            var message = isActive ? "Staff account activated successfully." : "Staff account deactivated successfully.";
            return ServiceResult<StaffResponse>.Ok(MapStaff(staff), message);
        }

        private async Task<StaffProfile?> FindStaffAsync(string userId)
        {
            return await _context.StaffProfiles
                .Include(staff => staff.User)
                .FirstOrDefaultAsync(staff => staff.UserId == userId);
        }

        private static StaffResponse MapStaff(StaffProfile staff)
        {
            return new StaffResponse
            {
                UserId = staff.UserId,
                StaffProfileId = staff.Id,
                FullName = staff.User.FullName,
                Email = staff.User.Email ?? string.Empty,
                PhoneNumber = staff.User.PhoneNumber,
                Role = staff.User.Role,
                IsActive = staff.User.IsActive,
                CreatedAt = staff.User.CreatedAt,
                StaffCode = staff.StaffCode,
                Position = staff.Position
            };
        }
    }
}
