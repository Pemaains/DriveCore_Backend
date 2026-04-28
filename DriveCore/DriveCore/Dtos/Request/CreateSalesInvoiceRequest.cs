using System.ComponentModel.DataAnnotations;

namespace DriveCore.Dtos.Request
{
    public class CreateSalesInvoiceRequest
    {
        [Range(1, int.MaxValue)]
        public int CustomerProfileId { get; set; }

        public int? VehicleId { get; set; }

        [MinLength(1, ErrorMessage = "At least one item is required.")]
        public List<CreateSalesInvoiceItemRequest> Items { get; set; } = new();
    }
}
