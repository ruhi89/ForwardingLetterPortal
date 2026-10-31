using ForwardingLetterPortal.Interface.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace ForwardingLetterPortal.Service
{
    public class EmailService(IConfiguration config) : IEmailService
    {
        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var section = config.GetSection("Email");

            using var client = new SmtpClient(section["Host"], int.Parse(section["Port"] ?? "587"))
            {
                EnableSsl = bool.Parse(section["EnableSsl"] ?? "true"),
                Credentials = new NetworkCredential(section["Username"], section["Password"])
            };

            using var message = new MailMessage(section["From"] ?? section["Username"]!, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
        }
    }
}
