using System.ComponentModel.DataAnnotations;

namespace DriveCore.Dtos.Request
{
    public class CreatePurchaseInvoiceRequest
    {
        [Required]
        [MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int VendorId { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        [MinLength(1)]
        public List<CreatePurchaseInvoiceItemRequest> Items { get; set; } = new();
    }

    public class CreatePurchaseInvoiceItemRequest
    {
        [Range(1, int.MaxValue)]
        public int PartId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitCost { get; set; }
    }
}
