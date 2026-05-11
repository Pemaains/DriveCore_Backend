using DriveCore.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace DriveCore.Services.Implementations
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public SmtpEmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.FromAddress))
            {
                throw new InvalidOperationException("Email settings are not configured.");
            }

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromAddress, _settings.FromName),
                Subject = subject,
                Body = body
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(_settings.UserName))
            {
                client.Credentials = new NetworkCredential(_settings.UserName, _settings.Password);
            }

            await client.SendMailAsync(message);
        }

        public Task SendEmailAsync(string toEmail, string toName, string subject, string body)
        {
            return SendAsync(toEmail, subject, body);
        }

        public Task SendCreditReminderAsync(string toEmail, string toName, decimal amountOwed, DateTime dueDate)
        {
            var subject = "DriveCore — Overdue Credit Payment Reminder";
            var body = $"Hello {toName},\n\nYour outstanding balance is {amountOwed:C} and was due on {dueDate:yyyy-MM-dd}.\n\nPlease settle this balance at your earliest convenience.\n\nDriveCore";
            return SendAsync(toEmail, subject, body);
        }
    }
}
