using DriveCore.Data;
using DriveCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoyaltyController(AppDbContext context) : ControllerBase
    {
        // GET: api/loyalty/{customerId}
        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetByCustomer(string customerId)
        {
            var loyalty = await context.LoyaltyPrograms
                .FirstOrDefaultAsync(l => l.CustomerId == customerId);

            if (loyalty == null)
            {
                return Ok(new LoyaltyProgram
                {
                    CustomerId = customerId,
                    TotalSpent = 0,
                    IsEligible = false,
                    DiscountPercentage = 10
                });
            }

            return Ok(loyalty);
        }

        // GET: api/loyalty
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var loyalties = await context.LoyaltyPrograms
                .OrderByDescending(l => l.TotalSpent)
                .ToListAsync();
            return Ok(loyalties);
        }

        // POST: api/loyalty/purchase
        [HttpPost("purchase")]
        public async Task<IActionResult> RecordPurchase([FromBody] PurchaseRequest request)
        {
            var loyalty = await context.LoyaltyPrograms
                .FirstOrDefaultAsync(l => l.CustomerId == request.CustomerId);

            if (loyalty == null)
            {
                loyalty = new LoyaltyProgram
                {
                    CustomerId = request.CustomerId,
                    TotalSpent = 0,
                    IsEligible = false,
                    DiscountPercentage = 10
                };
                context.LoyaltyPrograms.Add(loyalty);
            }

            loyalty.TotalSpent += request.PurchaseAmount;
            loyalty.IsEligible = request.PurchaseAmount >= 5000;
            loyalty.LastUpdated = DateTime.UtcNow;

            await context.SaveChangesAsync();

            var discountApplied = loyalty.IsEligible ? request.PurchaseAmount * 0.10m : 0;
            var finalAmount = request.PurchaseAmount - discountApplied;

            return Ok(new
            {
                customerId = request.CustomerId,
                purchaseAmount = request.PurchaseAmount,
                discountApplied = discountApplied,
                finalAmount = finalAmount,
                isEligible = loyalty.IsEligible,
                totalSpent = loyalty.TotalSpent,
                message = loyalty.IsEligible
                    ? $"10% discount applied! You save £{discountApplied:F2}"
                    : "Purchase recorded. Spend £5000 or more in a single purchase to get 10% discount."
            });
        }

        // GET: api/loyalty/eligible
        [HttpGet("eligible")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetEligibleCustomers()
        {
            var eligible = await context.LoyaltyPrograms
                .Where(l => l.IsEligible)
                .OrderByDescending(l => l.TotalSpent)
                .ToListAsync();
            return Ok(eligible);
        }
    }

    public class PurchaseRequest
    {
        public string CustomerId { get; set; } = string.Empty;
        public decimal PurchaseAmount { get; set; }
    }
}