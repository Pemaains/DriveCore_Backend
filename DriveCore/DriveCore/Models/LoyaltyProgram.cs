namespace DriveCore.Models
{
    public class LoyaltyProgram
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; } = 0;
        public bool IsEligible { get; set; } = false;
        public decimal DiscountPercentage { get; set; } = 10;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}