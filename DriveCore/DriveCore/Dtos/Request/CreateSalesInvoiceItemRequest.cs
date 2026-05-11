using System.ComponentModel.DataAnnotations;

namespace DriveCore.Dtos.Request
{
    public class CreateSalesInvoiceItemRequest
    {
        [Range(1, int.MaxValue)]
        public int PartId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
