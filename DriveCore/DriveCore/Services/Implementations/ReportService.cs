using DriveCore.Data;
using DriveCore.Dtos.Response;
using DriveCore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DriveCore.Services.Implementations;

public class ReportService(AppDbContext context) : IReportService
{
    public async Task<FinancialReportResponse> GetDailyFinancialReportAsync(DateTime? date)
    {
        var selectedDate = (date ?? DateTime.UtcNow).Date;
        var start = DateTime.SpecifyKind(selectedDate, DateTimeKind.Utc);
        var end = start.AddDays(1);

        var invoices = await GetInvoiceMetricsAsync(start, end);
        var breakdown = Enumerable.Range(0, 24)
            .Select(hour =>
            {
                var periodStart = start.AddHours(hour);
                var periodEnd = periodStart.AddHours(1);
                var hourInvoices = invoices
                    .Where(invoice => invoice.CreatedAt >= periodStart && invoice.CreatedAt < periodEnd)
                    .ToList();

                return BuildBreakdown(
                    $"{hour:00}:00",
                    periodStart,
                    periodEnd,
                    hourInvoices);
            })
            .Where(item => item.InvoiceCount > 0)
            .ToList();

        return BuildFinancialReport("Daily", start, end, invoices, breakdown);
    }

    public async Task<FinancialReportResponse> GetMonthlyFinancialReportAsync(int? year, int? month)
    {
        var now = DateTime.UtcNow;
        var selectedYear = NormalizeYear(year, now.Year);
        var selectedMonth = month is >= 1 and <= 12 ? month.Value : now.Month;

        var start = new DateTime(selectedYear, selectedMonth, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1);
        var invoices = await GetInvoiceMetricsAsync(start, end);
        var daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);

        var breakdown = Enumerable.Range(0, daysInMonth)
            .Select(offset =>
            {
                var periodStart = start.AddDays(offset);
                var periodEnd = periodStart.AddDays(1);
                var dayInvoices = invoices
                    .Where(invoice => invoice.CreatedAt >= periodStart && invoice.CreatedAt < periodEnd)
                    .ToList();

                return BuildBreakdown(
                    periodStart.ToString("yyyy-MM-dd"),
                    periodStart,
                    periodEnd,
                    dayInvoices);
            })
            .Where(item => item.InvoiceCount > 0)
            .ToList();

        return BuildFinancialReport("Monthly", start, end, invoices, breakdown);
    }

    public async Task<FinancialReportResponse> GetYearlyFinancialReportAsync(int? year)
    {
        var selectedYear = NormalizeYear(year, DateTime.UtcNow.Year);
        var start = new DateTime(selectedYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddYears(1);
        var invoices = await GetInvoiceMetricsAsync(start, end);

        var breakdown = Enumerable.Range(1, 12)
            .Select(month =>
            {
                var periodStart = new DateTime(selectedYear, month, 1, 0, 0, 0, DateTimeKind.Utc);
                var periodEnd = periodStart.AddMonths(1);
                var monthInvoices = invoices
                    .Where(invoice => invoice.CreatedAt >= periodStart && invoice.CreatedAt < periodEnd)
                    .ToList();

                return BuildBreakdown(
                    periodStart.ToString("MMMM"),
                    periodStart,
                    periodEnd,
                    monthInvoices);
            })
            .Where(item => item.InvoiceCount > 0)
            .ToList();

        return BuildFinancialReport("Yearly", start, end, invoices, breakdown);
    }

    public async Task<CustomerReportResponse> GetCustomerReportAsync(int topCount, int overdueAfterDays)
    {
        var normalizedTopCount = Math.Clamp(topCount, 1, 100);
        var normalizedOverdueAfterDays = Math.Clamp(overdueAfterDays, 1, 3650);
        var cutoffDate = DateTime.UtcNow.AddDays(-normalizedOverdueAfterDays);

        var customers = await context.CustomerProfiles
            .Include(profile => profile.User)
            .ToListAsync();

        var purchaseSummaries = await context.SalesInvoices
            .GroupBy(invoice => invoice.CustomerProfileId)
            .Select(group => new
            {
                CustomerProfileId = group.Key,
                InvoiceCount = group.Count(),
                TotalSpent = group.Sum(invoice => invoice.TotalAmount),
                LastPurchaseAt = group.Max(invoice => invoice.CreatedAt)
            })
            .ToListAsync();

        var overdueSummaries = await context.SalesInvoices
            .Where(invoice => invoice.CreatedAt <= cutoffDate)
            .GroupBy(invoice => invoice.CustomerProfileId)
            .Select(group => new
            {
                CustomerProfileId = group.Key,
                OverdueInvoiceCount = group.Count(),
                OutstandingAmount = group.Sum(invoice => invoice.TotalAmount),
                OldestInvoiceDate = group.Min(invoice => invoice.CreatedAt)
            })
            .ToListAsync();

        var customerKeys = customers
            .SelectMany(customer => new[] { customer.UserId, customer.Id.ToString() })
            .Distinct()
            .ToList();

        var appointments = await context.Appointments
            .Where(appointment => customerKeys.Contains(appointment.CustomerId))
            .ToListAsync();

        var appointmentsByCustomerId = customers.ToDictionary(
            customer => customer.Id,
            customer => appointments.Count(appointment =>
                appointment.CustomerId == customer.UserId
                || appointment.CustomerId == customer.Id.ToString()));

        var latestAppointmentByCustomerId = customers.ToDictionary(
            customer => customer.Id,
            customer => appointments
                .Where(appointment =>
                    appointment.CustomerId == customer.UserId
                    || appointment.CustomerId == customer.Id.ToString())
                .Select(appointment => (DateTime?)appointment.AppointmentDate)
                .OrderByDescending(date => date)
                .FirstOrDefault());

        var purchaseSummaryByCustomerId = purchaseSummaries.ToDictionary(item => item.CustomerProfileId);
        var overdueSummaryByCustomerId = overdueSummaries.ToDictionary(item => item.CustomerProfileId);

        var regularCustomers = customers
            .Select(customer =>
            {
                purchaseSummaryByCustomerId.TryGetValue(customer.Id, out var purchaseSummary);
                var appointmentCount = appointmentsByCustomerId.GetValueOrDefault(customer.Id);
                var lastAppointmentAt = latestAppointmentByCustomerId.GetValueOrDefault(customer.Id);
                var lastPurchaseAt = purchaseSummary?.LastPurchaseAt;
                var lastActivityAt = new[] { lastPurchaseAt, lastAppointmentAt }
                    .Where(date => date.HasValue)
                    .Select(date => date!.Value)
                    .DefaultIfEmpty()
                    .Max();

                var totalInteractions = (purchaseSummary?.InvoiceCount ?? 0) + appointmentCount;
                return new RegularCustomerReportItemResponse
                {
                    CustomerProfileId = customer.Id,
                    FullName = customer.User.FullName,
                    Email = customer.User.Email ?? string.Empty,
                    PhoneNumber = customer.User.PhoneNumber,
                    PurchaseCount = purchaseSummary?.InvoiceCount ?? 0,
                    AppointmentCount = appointmentCount,
                    TotalInteractions = totalInteractions,
                    TotalSpent = purchaseSummary?.TotalSpent ?? 0,
                    LastActivityAt = lastActivityAt == default ? null : lastActivityAt
                };
            })
            .Where(item => item.TotalInteractions >= 2)
            .OrderByDescending(item => item.TotalInteractions)
            .ThenByDescending(item => item.LastActivityAt)
            .Take(normalizedTopCount)
            .ToList();

        var highSpenders = customers
            .Select(customer =>
            {
                purchaseSummaryByCustomerId.TryGetValue(customer.Id, out var purchaseSummary);
                var invoiceCount = purchaseSummary?.InvoiceCount ?? 0;
                var totalSpent = purchaseSummary?.TotalSpent ?? 0;

                return new HighSpenderReportItemResponse
                {
                    CustomerProfileId = customer.Id,
                    FullName = customer.User.FullName,
                    Email = customer.User.Email ?? string.Empty,
                    PhoneNumber = customer.User.PhoneNumber,
                    TotalSpent = totalSpent,
                    InvoiceCount = invoiceCount,
                    AverageInvoiceValue = invoiceCount == 0 ? 0 : totalSpent / invoiceCount,
                    LastPurchaseAt = purchaseSummary?.LastPurchaseAt
                };
            })
            .Where(item => item.InvoiceCount > 0)
            .OrderByDescending(item => item.TotalSpent)
            .ThenByDescending(item => item.LastPurchaseAt)
            .Take(normalizedTopCount)
            .ToList();

        var pendingCredits = customers
            .Where(customer => overdueSummaryByCustomerId.ContainsKey(customer.Id))
            .Select(customer =>
            {
                var overdueSummary = overdueSummaryByCustomerId[customer.Id];
                return new PendingCreditReportItemResponse
                {
                    CustomerProfileId = customer.Id,
                    FullName = customer.User.FullName,
                    Email = customer.User.Email ?? string.Empty,
                    PhoneNumber = customer.User.PhoneNumber,
                    OverdueInvoiceCount = overdueSummary.OverdueInvoiceCount,
                    OutstandingAmount = overdueSummary.OutstandingAmount,
                    OldestInvoiceDate = overdueSummary.OldestInvoiceDate,
                    DaysOutstanding = Math.Max(0, (DateTime.UtcNow.Date - overdueSummary.OldestInvoiceDate.Date).Days)
                };
            })
            .OrderByDescending(item => item.OutstandingAmount)
            .ThenByDescending(item => item.DaysOutstanding)
            .Take(normalizedTopCount)
            .ToList();

        return new CustomerReportResponse
        {
            GeneratedAt = DateTime.UtcNow,
            OverdueAfterDays = normalizedOverdueAfterDays,
            RegularCustomers = regularCustomers,
            HighSpenders = highSpenders,
            PendingCredits = pendingCredits
        };
    }

    private async Task<List<InvoiceMetric>> GetInvoiceMetricsAsync(DateTime start, DateTime end)
    {
        return await context.SalesInvoices
            .Where(invoice => invoice.CreatedAt >= start && invoice.CreatedAt < end)
            .Select(invoice => new InvoiceMetric
            {
                CreatedAt = invoice.CreatedAt,
                TotalAmount = invoice.TotalAmount,
                ItemsSold = invoice.Items.Sum(item => item.Quantity)
            })
            .ToListAsync();
    }

    private static FinancialReportResponse BuildFinancialReport(
        string period,
        DateTime periodStart,
        DateTime periodEnd,
        List<InvoiceMetric> invoices,
        List<FinancialReportBreakdownResponse> breakdown)
    {
        var totalRevenue = invoices.Sum(invoice => invoice.TotalAmount);
        var invoiceCount = invoices.Count;

        return new FinancialReportResponse
        {
            Period = period,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            GeneratedAt = DateTime.UtcNow,
            TotalRevenue = totalRevenue,
            InvoiceCount = invoiceCount,
            ItemsSold = invoices.Sum(invoice => invoice.ItemsSold),
            AverageInvoiceValue = invoiceCount == 0 ? 0 : totalRevenue / invoiceCount,
            Breakdown = breakdown
        };
    }

    private static FinancialReportBreakdownResponse BuildBreakdown(
        string label,
        DateTime periodStart,
        DateTime periodEnd,
        List<InvoiceMetric> invoices)
    {
        return new FinancialReportBreakdownResponse
        {
            Label = label,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Revenue = invoices.Sum(invoice => invoice.TotalAmount),
            InvoiceCount = invoices.Count,
            ItemsSold = invoices.Sum(invoice => invoice.ItemsSold)
        };
    }

    private static int NormalizeYear(int? year, int fallbackYear)
    {
        return year is >= 2000 and <= 2100
            ? year.Value
            : fallbackYear;
    }

    private sealed class InvoiceMetric
    {
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemsSold { get; set; }
    }
}