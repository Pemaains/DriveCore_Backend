namespace DriveCore.Dtos.Response
{
    public class CustomerDetailResponse
    {
        public string UserId { get; set; } = string.Empty;
        public int CustomerProfileId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? CreatedByStaffId { get; set; }
        public List<VehicleResponse> Vehicles { get; set; } = new();
        public List<SalesInvoiceSummaryResponse> Invoices { get; set; } = new();
    }
}
