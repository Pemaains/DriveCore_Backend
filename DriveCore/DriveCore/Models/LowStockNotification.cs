namespace DriveCore.Models
{
    public class LowStockNotification
    {
        public int Id { get; set; }
        public string PartName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int Threshold { get; set; } = 10;
        public DateTime NotifiedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}