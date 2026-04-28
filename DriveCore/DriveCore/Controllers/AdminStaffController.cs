using DriveCore.Dtos.Request;
using DriveCore.Dtos.Response;
using DriveCore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveCore.Controllers
{
    [ApiController]
    [Route("api/admin/staff")]
    [Authorize(Roles = "Admin")]
    public class AdminStaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public AdminStaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStaff(CreateStaffRequest request)
        {
            var result = await _staffService.CreateStaffAsync(request);
            if (!result.Success)
            {
                return BadRequest(ToErrorResponse(result));
            }

            return CreatedAtAction(nameof(GetStaffById), new { id = result.Data!.UserId }, result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStaff()
        {
            var staff = await _staffService.GetAllStaffAsync();
            return Ok(staff);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStaffById(string id)
        {
            var result = await _staffService.GetStaffByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(ToErrorResponse(result));
            }

            return Ok(result.Data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaff(string id, UpdateStaffRequest request)
        {
            var result = await _staffService.UpdateStaffAsync(id, request);
            if (!result.Success)
            {
                return NotFound(ToErrorResponse(result));
            }

            return Ok(result.Data);
        }

        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateStaffRole(string id, UpdateStaffRoleRequest request)
        {
            var result = await _staffService.UpdateStaffRoleAsync(id, request.Role);
            if (!result.Success)
            {
                return BadRequest(ToErrorResponse(result));
            }

            return Ok(result.Data);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStaffStatus(string id, UpdateStaffStatusRequest request)
        {
            var result = await _staffService.UpdateStaffStatusAsync(id, request.IsActive);
            if (!result.Success)
            {
                return NotFound(ToErrorResponse(result));
            }

            return Ok(result.Data);
        }

        private static ErrorResponse ToErrorResponse<T>(Services.ServiceResult<T> result)
        {
            return new ErrorResponse
            {
                Message = result.Message,
                Errors = result.Errors
            };
        }
    }
}
