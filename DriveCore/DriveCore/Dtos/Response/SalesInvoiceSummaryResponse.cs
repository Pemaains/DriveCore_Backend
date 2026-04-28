namespace DriveCore.Dtos.Response
{
    public class SalesInvoiceSummaryResponse
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public int? VehicleId { get; set; }
    }
}
