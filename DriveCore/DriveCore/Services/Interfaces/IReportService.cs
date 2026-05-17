using DriveCore.Dtos.Response;

namespace DriveCore.Services.Interfaces;

public interface IReportService
{
    Task<FinancialReportResponse> GetDailyFinancialReportAsync(DateTime? date);
    Task<FinancialReportResponse> GetMonthlyFinancialReportAsync(int? year, int? month);
    Task<FinancialReportResponse> GetYearlyFinancialReportAsync(int? year);
    Task<CustomerReportResponse> GetCustomerReportAsync(int topCount, int overdueAfterDays);
}