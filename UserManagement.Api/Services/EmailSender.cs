using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Threading.Tasks;
using UserManagement.Api.Services.Interfaces;

namespace UserManagement.Api.Services
{
    /// <summary>
    /// Implements email delivery services utilizing MailKit SmtpClient over secure TLS transport 
    /// configured by app settings (e.g. connecting to SendGrid SMTP).
    /// </summary>
    public class EmailSender(IConfiguration config) : IEmailSender
    {
        private readonly IConfiguration _config = config;

        /// <summary>
        /// Composes and sends a secure HTML email message to the specified recipient.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="subject">The email subject header.</param>
        /// <param name="htmlMessage">The email body formatted as HTML.</param>
        /// <returns>A task representing the asynchronous email dispatch operation.</returns>
        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            IConfigurationSection emailSettings = _config.GetSection("EmailSettings");
            
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailSettings["SenderName"] ?? "Portal", emailSettings["SenderEmail"] ?? string.Empty));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            BodyBuilder bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            message.Body = bodyBuilder.ToMessageBody();

            using SmtpClient client = new SmtpClient();
            
            // Connect to SendGrid SMTP server using StartTLS on port 587
            await client.ConnectAsync(emailSettings["SmtpServer"] ?? string.Empty, int.Parse(emailSettings["Port"] ?? "587"), MailKit.Security.SecureSocketOptions.StartTls);
            
            // Authenticate using the username "apikey" and the SendGrid API Key as the password
            await client.AuthenticateAsync(emailSettings["Username"] ?? string.Empty, emailSettings["Password"] ?? string.Empty);
            
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
