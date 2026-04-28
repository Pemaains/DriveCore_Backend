namespace DriveCore.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}