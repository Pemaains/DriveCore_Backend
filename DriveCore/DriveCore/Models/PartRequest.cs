namespace DriveCore.Models
{
    public class PartRequest
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }
}