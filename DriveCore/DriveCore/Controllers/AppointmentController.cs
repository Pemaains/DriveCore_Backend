using DriveCore.Data;
using DriveCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController(AppDbContext context) : ControllerBase
    {
        // GET: api/appointment
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var appointments = await context.Appointments.ToListAsync();
            return Ok(appointments);
        }

        // GET: api/appointment/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var appointment = await context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();
            return Ok(appointment);
        }

        // POST: api/appointment
        [HttpPost]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
        }

        // PUT: api/appointment/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Appointment updated)
        {
            var appointment = await context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.AppointmentDate = updated.AppointmentDate;
            appointment.ServiceType = updated.ServiceType;
            appointment.Status = updated.Status;
            appointment.Notes = updated.Notes;

            await context.SaveChangesAsync();
            return Ok(appointment);
        }

        // DELETE: api/appointment/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            context.Appointments.Remove(appointment);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}