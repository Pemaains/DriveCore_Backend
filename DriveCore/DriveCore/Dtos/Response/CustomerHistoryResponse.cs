namespace DriveCore.Dtos.Response;

public class CustomerHistoryResponse
{
    public int CustomerProfileId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public int TotalPurchaseCount { get; set; }
    public decimal TotalPurchaseAmount { get; set; }
    public int TotalServiceCount { get; set; }
    public List<PurchaseHistoryItemResponse> Purchases { get; set; } = new();
    public List<ServiceHistoryItemResponse> Services { get; set; } = new();
}

public class PurchaseHistoryItemResponse
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public int? VehicleId { get; set; }
    public string? VehicleNumber { get; set; }
    public List<PurchaseHistoryLineItemResponse> Items { get; set; } = new();
}

public class PurchaseHistoryLineItemResponse
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class ServiceHistoryItemResponse
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}