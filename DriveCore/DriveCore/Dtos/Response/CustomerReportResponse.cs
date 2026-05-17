namespace DriveCore.Dtos.Response;

public class CustomerReportResponse
{
    public DateTime GeneratedAt { get; set; }
    public int OverdueAfterDays { get; set; }
    public List<RegularCustomerReportItemResponse> RegularCustomers { get; set; } = new();
    public List<HighSpenderReportItemResponse> HighSpenders { get; set; } = new();
    public List<PendingCreditReportItemResponse> PendingCredits { get; set; } = new();
}

public class RegularCustomerReportItemResponse
{
    public int CustomerProfileId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int PurchaseCount { get; set; }
    public int AppointmentCount { get; set; }
    public int TotalInteractions { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastActivityAt { get; set; }
}

public class HighSpenderReportItemResponse
{
    public int CustomerProfileId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public decimal TotalSpent { get; set; }
    public int InvoiceCount { get; set; }
    public decimal AverageInvoiceValue { get; set; }
    public DateTime? LastPurchaseAt { get; set; }
}

public class PendingCreditReportItemResponse
{
    public int CustomerProfileId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int OverdueInvoiceCount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public DateTime OldestInvoiceDate { get; set; }
    public int DaysOutstanding { get; set; }
}