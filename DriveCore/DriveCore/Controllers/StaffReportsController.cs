using DriveCore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveCore.Controllers;

[ApiController]
[Authorize(Roles = "Staff")]
[Route("api/staff/reports")]
public class StaffReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomerReports([FromQuery] int topCount = 10, [FromQuery] int overdueAfterDays = 30)
    {
        var report = await reportService.GetCustomerReportAsync(topCount, overdueAfterDays);
        return Ok(report);
    }
}