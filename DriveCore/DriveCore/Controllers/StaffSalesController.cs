using DriveCore.Dtos.Request;
using DriveCore.Dtos.Response;
using DriveCore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DriveCore.Controllers
{
    [ApiController]
    [Route("api/staff/sales")]
    public class StaffSalesController : ControllerBase
    {
        private readonly ISalesService _salesService;

        public StaffSalesController(ISalesService salesService)
        {
            _salesService = salesService;
        }

        [HttpPost("parts")]
        public async Task<IActionResult> CreatePart(CreatePartRequest request)
        {
            var result = await _salesService.CreatePartAsync(request);
            if (!result.Success)
            {
                return BadRequest(ToErrorResponse(result));
            }

            return Created(string.Empty, result.Data);
        }

        [HttpGet("parts")]
        public async Task<IActionResult> GetParts()
        {
            var parts = await _salesService.GetPartsAsync();
            return Ok(parts);
        }

        [HttpPost("invoices")]
        public async Task<IActionResult> CreateInvoice(CreateSalesInvoiceRequest request)
        {
            var staffUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(staffUserId))
            {
                return Unauthorized();
            }

            var result = await _salesService.CreateInvoiceAsync(request, staffUserId);
            if (!result.Success)
            {
                return BadRequest(ToErrorResponse(result));
            }

            return CreatedAtAction(nameof(GetInvoiceById), new { id = result.Data!.Id }, result.Data);
        }

        [HttpGet("invoices/{id:int}")]
        public async Task<IActionResult> GetInvoiceById(int id)
        {
            var result = await _salesService.GetInvoiceByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(ToErrorResponse(result));
            }

            return Ok(result.Data);
        }

        [HttpPost("invoices/{id:int}/send")]
        public async Task<IActionResult> SendInvoice(int id)
        {
            var result = await _salesService.SendInvoiceAsync(id);
            if (!result.Success)
            {
                return BadRequest(ToErrorResponse(result));
            }

            return Ok(new { message = result.Message });
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
