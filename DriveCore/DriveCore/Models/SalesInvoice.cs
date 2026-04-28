using System.ComponentModel.DataAnnotations;

namespace DriveCore.Models
{
    public class SalesInvoice
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string InvoiceNumber { get; set; } = string.Empty;

        public int CustomerProfileId { get; set; }

        public CustomerProfile CustomerProfile { get; set; } = null!;

        public int StaffProfileId { get; set; }

        public StaffProfile StaffProfile { get; set; } = null!;

        public int? VehicleId { get; set; }

        public Vehicle? Vehicle { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        public ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
    }
}
