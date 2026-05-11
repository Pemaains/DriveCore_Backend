namespace DriveCore.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string toName, string subject, string body);
        Task SendCreditReminderAsync(string toEmail, string toName, decimal amountOwed, DateTime dueDate);
        Task SendAsync(string toEmail, string subject, string body);
    }
}