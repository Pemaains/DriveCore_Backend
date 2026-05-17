namespace DriveCore.Dtos.Response;

public class FinancialReportResponse
{
    public string Period { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime GeneratedAt { get; set; }
    public decimal TotalRevenue { get; set; }
    public int InvoiceCount { get; set; }
    public int ItemsSold { get; set; }
    public decimal AverageInvoiceValue { get; set; }
    public List<FinancialReportBreakdownResponse> Breakdown { get; set; } = new();
}

public class FinancialReportBreakdownResponse
{
    public string Label { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal Revenue { get; set; }
    public int InvoiceCount { get; set; }
    public int ItemsSold { get; set; }
}