using DriveCore.Dtos.Request;
using DriveCore.Dtos.Response;
using DriveCore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DriveCore.Controllers
{
    [ApiController]
    [Route("api/staff/customers")]
    [Authorize(Roles = "Staff")]
    public class StaffCustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public StaffCustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer(CreateCustomerRequest request)
        {
            var staffUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _customerService.CreateCustomerAsync(request, staffUserId);

            if (!result.Success)
            {
                return BadRequest(ToErrorResponse(result));
            }

            return CreatedAtAction(nameof(GetCustomerById), new { id = result.Data!.CustomerProfileId }, result.Data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var result = await _customerService.GetCustomerByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(ToErrorResponse(result));
            }

            return Ok(result.Data);
        }

        [HttpPost("{customerId:int}/vehicles")]
        public async Task<IActionResult> AddVehicle(int customerId, CreateVehicleRequest request)
        {
            var result = await _customerService.AddVehicleAsync(customerId, request);
            if (!result.Success)
            {
                return BadRequest(ToErrorResponse(result));
            }

            return Created(string.Empty, result.Data);
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
