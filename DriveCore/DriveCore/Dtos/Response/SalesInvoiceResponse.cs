namespace DriveCore.Dtos.Response
{
    public class SalesInvoiceResponse
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public int CustomerProfileId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public int? VehicleId { get; set; }
        public List<SalesInvoiceItemResponse> Items { get; set; } = new();
    }
}
