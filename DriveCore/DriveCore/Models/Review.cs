namespace DriveCore.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}