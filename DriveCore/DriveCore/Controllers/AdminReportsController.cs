using Microsoft.AspNetCore.Mvc;
using DriveCore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace DriveCore.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/reports")]
public class AdminReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("financial/daily")]
    public async Task<IActionResult> GetDailyFinancialReport([FromQuery] DateTime? date)
    {
        var report = await reportService.GetDailyFinancialReportAsync(date);
        return Ok(report);
    }

    [HttpGet("financial/monthly")]
    public async Task<IActionResult> GetMonthlyFinancialReport([FromQuery] int? year, [FromQuery] int? month)
    {
        var report = await reportService.GetMonthlyFinancialReportAsync(year, month);
        return Ok(report);
    }

    [HttpGet("financial/yearly")]
    public async Task<IActionResult> GetYearlyFinancialReport([FromQuery] int? year)
    {
        var report = await reportService.GetYearlyFinancialReportAsync(year);
        return Ok(report);
    }
}