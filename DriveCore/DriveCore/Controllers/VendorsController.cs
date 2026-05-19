using DriveCore.Data;
using DriveCore.Dtos.Request;
using DriveCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorsController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vendors = await context.Vendors
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
            return Ok(vendors);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vendor = await context.Vendors.FindAsync(id);
            return vendor is null ? NotFound() : Ok(vendor);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VendorUpsertRequest request)
        {
            var vendor = new Vendor
            {
                Name = request.Name.Trim(),
                ContactPerson = request.ContactPerson?.Trim() ?? string.Empty,
                Phone = request.Phone?.Trim() ?? string.Empty,
                Email = request.Email?.Trim() ?? string.Empty,
                Address = request.Address?.Trim() ?? string.Empty,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            context.Vendors.Add(vendor);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = vendor.Id }, vendor);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] VendorUpsertRequest request)
        {
            var vendor = await context.Vendors.FindAsync(id);
            if (vendor is null) return NotFound();

            vendor.Name = request.Name.Trim();
            vendor.ContactPerson = request.ContactPerson?.Trim() ?? string.Empty;
            vendor.Phone = request.Phone?.Trim() ?? string.Empty;
            vendor.Email = request.Email?.Trim() ?? string.Empty;
            vendor.Address = request.Address?.Trim() ?? string.Empty;
            vendor.IsActive = request.IsActive;

            await context.SaveChangesAsync();
            return Ok(vendor);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vendor = await context.Vendors.FindAsync(id);
            if (vendor is null) return NotFound();

            var hasInvoices = await context.PurchaseInvoices.AnyAsync(i => i.VendorId == id);
            if (hasInvoices)
            {
                return BadRequest("Vendor cannot be deleted because purchase invoices are linked.");
            }

            context.Vendors.Remove(vendor);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
