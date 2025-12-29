using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace Taxi_API.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var msg = new MimeMessage();
            msg.From.Add(MailboxAddress.Parse(_config["Smtp:From"] ?? "no-reply@example.com"));
            msg.To.Add(MailboxAddress.Parse(to));
            msg.Subject = subject;
            msg.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"] ?? "587"), false);
            await client.AuthenticateAsync(_config["Smtp:Username"], _config["Smtp:Password"]);
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
        }
    }
}