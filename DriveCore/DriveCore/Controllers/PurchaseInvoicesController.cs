using DriveCore.Data;
using DriveCore.Dtos.Request;
using DriveCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseInvoicesController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var invoices = await context.PurchaseInvoices
                .Include(i => i.Vendor)
                .Include(i => i.Items)
                    .ThenInclude(item => item.Part)
                .OrderByDescending(i => i.PurchaseDate)
                .ToListAsync();
            return Ok(invoices);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await context.PurchaseInvoices
                .Include(i => i.Vendor)
                .Include(i => i.Items)
                    .ThenInclude(item => item.Part)
                .FirstOrDefaultAsync(i => i.Id == id);
            return invoice is null ? NotFound() : Ok(invoice);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseInvoiceRequest request)
        {
            var invoiceExists = await context.PurchaseInvoices
                .AnyAsync(i => i.InvoiceNumber.ToLower() == request.InvoiceNumber.Trim().ToLower());
            if (invoiceExists)
            {
                return BadRequest("Invoice number already exists.");
            }

            var vendorExists = await context.Vendors.AnyAsync(v => v.Id == request.VendorId);
            if (!vendorExists)
            {
                return BadRequest("Vendor not found.");
            }

            var partIds = request.Items.Select(i => i.PartId).Distinct().ToList();
            var parts = await context.Parts.Where(p => partIds.Contains(p.Id)).ToListAsync();
            if (parts.Count != partIds.Count)
            {
                return BadRequest("One or more parts are invalid.");
            }

            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var invoice = new PurchaseInvoice
                {
                    InvoiceNumber = request.InvoiceNumber.Trim(),
                    VendorId = request.VendorId,
                    PurchaseDate = request.PurchaseDate == default ? DateTime.UtcNow : request.PurchaseDate,
                    Notes = request.Notes?.Trim() ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                decimal total = 0m;
                foreach (var item in request.Items)
                {
                    var part = parts.First(p => p.Id == item.PartId);
                    var lineTotal = item.Quantity * item.UnitCost;
                    total += lineTotal;

                    invoice.Items.Add(new PurchaseInvoiceItem
                    {
                        PartId = item.PartId,
                        Quantity = item.Quantity,
                        UnitCost = item.UnitCost,
                        LineTotal = lineTotal
                    });

                    part.StockQuantity += item.Quantity;
                    part.UpdatedAt = DateTime.UtcNow;
                }

                invoice.TotalAmount = total;
                context.PurchaseInvoices.Add(invoice);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
