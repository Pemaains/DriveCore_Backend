using System.ComponentModel.DataAnnotations;

namespace DriveCore.Models
{
    public class SalesInvoiceItem
    {
        public int Id { get; set; }

        public int SalesInvoiceId { get; set; }

        public SalesInvoice SalesInvoice { get; set; } = null!;

        public int PartId { get; set; }

        public Part Part { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal LineTotal { get; set; }
    }
}
