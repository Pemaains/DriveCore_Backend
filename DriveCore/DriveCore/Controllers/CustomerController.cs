using DriveCore.Dtos.Request;
using DriveCore.Dtos.Response;
using DriveCore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DriveCore.Controllers
{
    [ApiController]
    [Route("api/customer")]
    [Authorize(Roles = "Customer")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _customerService.GetCurrentCustomerAsync(GetUserId());
            if (!result.Success)
            {
                return NotFound(ToErrorResponse(result));
            }

            return Ok(result.Data);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile(UpdateCustomerProfileRequest request)
        {
            var result = await _customerService.UpdateCurrentCustomerAsync(GetUserId(), request);
            if (!result.Success)
            {
                return BadRequest(ToErrorResponse(result));
            }

            return Ok(result.Data);
        }

        [HttpPost("me/vehicles")]
        public async Task<IActionResult> AddVehicle(CreateVehicleRequest request)
        {
            var result = await _customerService.AddCurrentCustomerVehicleAsync(GetUserId(), request);
            if (!result.Success)
            {
                return BadRequest(ToErrorResponse(result));
            }

            return Created(string.Empty, result.Data);
        }

        [HttpPut("me/vehicles/{vehicleId:int}")]
        public async Task<IActionResult> UpdateVehicle(int vehicleId, UpdateVehicleRequest request)
        {
            var result = await _customerService.UpdateCurrentCustomerVehicleAsync(GetUserId(), vehicleId, request);
            if (!result.Success)
            {
                return BadRequest(ToErrorResponse(result));
            }

            return Ok(result.Data);
        }

        [HttpDelete("me/vehicles/{vehicleId:int}")]
        public async Task<IActionResult> DeleteVehicle(int vehicleId)
        {
            var result = await _customerService.DeleteCurrentCustomerVehicleAsync(GetUserId(), vehicleId);
            if (!result.Success)
            {
                return BadRequest(ToErrorResponse(result));
            }

            return NoContent();
        }

        [HttpGet("me/history")]
        public async Task<IActionResult> GetHistory()
        {
            var result = await _customerService.GetCurrentCustomerHistoryAsync(GetUserId());
            if (!result.Success)
            {
                return NotFound(ToErrorResponse(result));
            }

            return Ok(result.Data);
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
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
