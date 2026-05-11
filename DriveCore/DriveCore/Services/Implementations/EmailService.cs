using DriveCore.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace DriveCore.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string body)
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(
                _configuration["Email:FromName"] ?? "DriveCore",
                _configuration["Email:FromAddress"]
            ));

            email.To.Add(new MailboxAddress(toName, toEmail));
            email.Subject = subject;

            email.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _configuration["Email:Host"],
                int.Parse(_configuration["Email:Port"] ?? "587"),
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                _configuration["Email:Username"],
                _configuration["Email:Password"]
            );

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendCreditReminderAsync(string toEmail, string toName, decimal amountOwed, DateTime dueDate)
        {
            var subject = "DriveCore — Overdue Credit Payment Reminder";

            var body = $@"
                <div style='font-family: Georgia, serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: #1c1c1c; padding: 24px; text-align: center;'>
                        <h1 style='color: #fff; letter-spacing: 3px; margin: 0;'>DRIVECORE</h1>
                    </div>
                    <div style='padding: 32px; background: #f9f9f9;'>
                        <p style='color: #333;'>Dear {toName},</p>
                        <p style='color: #555;'>
                            This is a reminder that you have an outstanding credit balance of 
                            <strong style='color: #8a3a3a;'>£{amountOwed:F2}</strong> 
                            that was due on <strong>{dueDate:MMMM dd, yyyy}</strong>.
                        </p>
                        <p style='color: #555;'>
                            Your account has been overdue for more than one month. 
                            Please settle this balance at your earliest convenience to avoid any service disruptions.
                        </p>
                        <div style='background: #fff; border: 1px solid #e0e0e0; padding: 20px; margin: 24px 0; border-radius: 4px;'>
                            <p style='margin: 0; color: #333;'><strong>Amount Owed:</strong> £{amountOwed:F2}</p>
                            <p style='margin: 8px 0 0; color: #333;'><strong>Due Date:</strong> {dueDate:MMMM dd, yyyy}</p>
                            <p style='margin: 8px 0 0; color: #8a3a3a;'><strong>Status:</strong> Overdue</p>
                        </div>
                        <p style='color: #555;'>
                            If you have already made payment, please disregard this notice. 
                            Contact us if you have any questions.
                        </p>
                        <p style='color: #555;'>Thank you for your continued business.</p>
                        <p style='color: #333;'><strong>DriveCore Team</strong></p>
                    </div>
                    <div style='background: #1c1c1c; padding: 16px; text-align: center;'>
                        <p style='color: #666; font-size: 12px; margin: 0;'>
                            This is an automated reminder from DriveCore Vehicle Parts & Services.
                        </p>
                    </div>
                </div>
            ";

            await SendEmailAsync(toEmail, toName, subject, body);
        }
    }
}