using DriveCore.Data;
using DriveCore.Dtos.Request;
using DriveCore.Models;
using DriveCore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController(AppDbContext context) : ControllerBase
    {
        // GET: api/notification/low-stock
        [HttpGet("low-stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLowStockNotifications()
        {
            var notifications = await context.LowStockNotifications
                .OrderByDescending(n => n.NotifiedAt)
                .ToListAsync();
            return Ok(notifications);
        }

        // POST: api/notification/send-credit-reminder
        [HttpPost("send-credit-reminder")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendCreditReminder(
            [FromServices] IEmailService emailService,
            [FromBody] CreditReminderRequest request)
        {
            try
            {
                await emailService.SendCreditReminderAsync(
                    request.ToEmail,
                    request.ToName,
                    request.AmountOwed,
                    request.DueDate
                );
                return Ok(new { message = $"Reminder sent to {request.ToEmail}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to send email.", error = ex.Message });
            }
        }

        // GET: api/notification/low-stock/unread-count
        [HttpGet("low-stock/unread-count")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await context.LowStockNotifications
                .CountAsync(n => !n.IsRead);
            return Ok(new { count });
        }

        // POST: api/notification/low-stock
        [HttpPost("low-stock")]
        public async Task<IActionResult> CreateLowStockNotification(LowStockNotification notification)
        {
            notification.NotifiedAt = DateTime.UtcNow;
            context.LowStockNotifications.Add(notification);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLowStockNotifications), notification);
        }

        // PUT: api/notification/low-stock/{id}/read
        [HttpPut("low-stock/{id}/read")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await context.LowStockNotifications.FindAsync(id);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await context.SaveChangesAsync();
            return Ok(notification);
        }

        // PUT: api/notification/low-stock/mark-all-read
        [HttpPut("low-stock/mark-all-read")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var unread = await context.LowStockNotifications
                .Where(n => !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
                n.IsRead = true;

            await context.SaveChangesAsync();
            return Ok(new { marked = unread.Count });
        }

        // DELETE: api/notification/low-stock/{id}
        [HttpDelete("low-stock/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var notification = await context.LowStockNotifications.FindAsync(id);
            if (notification == null) return NotFound();

            context.LowStockNotifications.Remove(notification);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}