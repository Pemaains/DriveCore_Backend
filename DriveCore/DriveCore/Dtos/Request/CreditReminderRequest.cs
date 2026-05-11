namespace DriveCore.Dtos.Request
{
    public class CreditReminderRequest
    {
        public string ToEmail { get; set; } = string.Empty;
        public string ToName { get; set; } = string.Empty;
        public decimal AmountOwed { get; set; }
        public DateTime DueDate { get; set; }
    }
}