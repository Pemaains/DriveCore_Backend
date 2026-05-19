using DriveCore.Dtos.Response;

namespace DriveCore.Services.Interfaces;

/// <summary>
/// Defines reporting operations used by the admin and staff modules.
/// </summary>
public interface IReportService
{
    #region Financial Reports

    /// <summary>
    /// Generates a financial report for a single day.
    /// </summary>
    /// <param name="date">The target date. When null, the current UTC date is used.</param>
    /// <returns>A daily financial report with totals and hourly breakdown.</returns>
    Task<FinancialReportResponse> GetDailyFinancialReportAsync(DateTime? date);

    /// <summary>
    /// Generates a financial report for a specific month.
    /// </summary>
    /// <param name="year">The target year. When null or invalid, the current UTC year is used.</param>
    /// <param name="month">The target month. When null or invalid, the current UTC month is used.</param>
    /// <returns>A monthly financial report with totals and daily breakdown.</returns>
    Task<FinancialReportResponse> GetMonthlyFinancialReportAsync(int? year, int? month);

    /// <summary>
    /// Generates a financial report for a specific year.
    /// </summary>
    /// <param name="year">The target year. When null or invalid, the current UTC year is used.</param>
    /// <returns>A yearly financial report with totals and monthly breakdown.</returns>
    Task<FinancialReportResponse> GetYearlyFinancialReportAsync(int? year);

    #endregion

    #region Customer Reports

    /// <summary>
    /// Generates grouped customer insights for staff reporting.
    /// </summary>
    /// <param name="topCount">The maximum number of records returned in each report section.</param>
    /// <param name="overdueAfterDays">The age threshold used to classify invoices as overdue for follow-up.</param>
    /// <returns>A customer report containing regular customers, high spenders, and pending credits.</returns>
    Task<CustomerReportResponse> GetCustomerReportAsync(int topCount, int overdueAfterDays);

    #endregion
}
