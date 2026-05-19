using DriveCore.Data;
using DriveCore.Dtos.Request;
using DriveCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartsController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var query = context.Parts
                .Include(p => p.PreferredVendor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    p.PartNumber.ToLower().Contains(term) ||
                    p.Description.ToLower().Contains(term));
            }

            var parts = await query
                .OrderByDescending(p => p.UpdatedAt)
                .ToListAsync();

            return Ok(parts);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var part = await context.Parts
                .Include(p => p.PreferredVendor)
                .FirstOrDefaultAsync(p => p.Id == id);
            return part is null ? NotFound() : Ok(part);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PartUpsertRequest request)
        {
            var duplicate = await context.Parts
                .AnyAsync(p => p.PartNumber.ToLower() == request.PartNumber.Trim().ToLower());
            if (duplicate)
            {
                return BadRequest("Part number already exists.");
            }

            if (request.PreferredVendorId.HasValue)
            {
                var vendorExists = await context.Vendors.AnyAsync(v => v.Id == request.PreferredVendorId.Value);
                if (!vendorExists) return BadRequest("Preferred vendor not found.");
            }

            var part = new Part
            {
                Name = request.Name.Trim(),
                PartNumber = request.PartNumber.Trim(),
                Description = request.Description?.Trim() ?? string.Empty,
                UnitPrice = request.UnitPrice,
                StockQuantity = request.StockQuantity,
                ReorderLevel = request.ReorderLevel,
                PreferredVendorId = request.PreferredVendorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Parts.Add(part);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = part.Id }, part);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] PartUpsertRequest request)
        {
            var part = await context.Parts.FindAsync(id);
            if (part is null) return NotFound();

            var duplicate = await context.Parts
                .AnyAsync(p => p.Id != id && p.PartNumber.ToLower() == request.PartNumber.Trim().ToLower());
            if (duplicate)
            {
                return BadRequest("Part number already exists.");
            }

            if (request.PreferredVendorId.HasValue)
            {
                var vendorExists = await context.Vendors.AnyAsync(v => v.Id == request.PreferredVendorId.Value);
                if (!vendorExists) return BadRequest("Preferred vendor not found.");
            }

            part.Name = request.Name.Trim();
            part.PartNumber = request.PartNumber.Trim();
            part.Description = request.Description?.Trim() ?? string.Empty;
            part.UnitPrice = request.UnitPrice;
            part.StockQuantity = request.StockQuantity;
            part.ReorderLevel = request.ReorderLevel;
            part.PreferredVendorId = request.PreferredVendorId;
            part.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return Ok(part);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var part = await context.Parts.FindAsync(id);
            if (part is null) return NotFound();

            var usedInInvoices = await context.PurchaseInvoiceItems.AnyAsync(i => i.PartId == id);
            if (usedInInvoices)
            {
                return BadRequest("Part cannot be deleted because invoice history exists.");
            }

            context.Parts.Remove(part);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
