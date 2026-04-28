using DriveCore.Data;
using DriveCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartRequestController(AppDbContext context) : ControllerBase
    {
        // GET: api/partrequest
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await context.PartRequests.ToListAsync();
            return Ok(requests);
        }

        // GET: api/partrequest/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var request = await context.PartRequests.FindAsync(id);
            if (request == null) return NotFound();
            return Ok(request);
        }

        // GET: api/partrequest/customer/{customerId}
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(string customerId)
        {
            var requests = await context.PartRequests
                .Where(r => r.CustomerId == customerId)
                .ToListAsync();
            return Ok(requests);
        }

        // POST: api/partrequest
        [HttpPost]
        public async Task<IActionResult> Create(PartRequest request)
        {
            context.PartRequests.Add(request);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
        }

        // PUT: api/partrequest/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var request = await context.PartRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = status;
            await context.SaveChangesAsync();
            return Ok(request);
        }

        // DELETE: api/partrequest/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await context.PartRequests.FindAsync(id);
            if (request == null) return NotFound();

            context.PartRequests.Remove(request);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}