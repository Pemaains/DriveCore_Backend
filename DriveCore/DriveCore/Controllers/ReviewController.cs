using DriveCore.Data;
using DriveCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController(AppDbContext context) : ControllerBase
    {
        // GET: api/review
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reviews = await context.Reviews.ToListAsync();
            return Ok(reviews);
        }

        // GET: api/review/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var review = await context.Reviews.FindAsync(id);
            if (review == null) return NotFound();
            return Ok(review);
        }

        // GET: api/review/customer/{customerId}
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(string customerId)
        {
            var reviews = await context.Reviews
                .Where(r => r.CustomerId == customerId)
                .ToListAsync();
            return Ok(reviews);
        }

        // POST: api/review
        [HttpPost]
        public async Task<IActionResult> Create(Review review)
        {
            if (review.Rating < 1 || review.Rating > 5)
                return BadRequest("Rating must be between 1 and 5");

            context.Reviews.Add(review);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = review.Id }, review);
        }

        // DELETE: api/review/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            context.Reviews.Remove(review);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}