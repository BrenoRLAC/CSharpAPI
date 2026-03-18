using API.Domain;
using API.Infrastructure.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace API.Infrastructure
{
    public class Mailer : IMailer
    {
        private readonly SmtpSettings _smtpSettings;

        public Mailer(SmtpSettings smtpSettings)
        {
            _smtpSettings = smtpSettings;
        }

        public async Task SendEmailAsync(string name, string email, string subject, string body,
            MimeEntity mimeEntity = null)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
            message.To.Add(new MailboxAddress(name, email));
            message.Subject = subject;

            message.Body = mimeEntity ?? new TextPart("html")
            {
                Text = string.Format(body)
            };

            using var client = new SmtpClient();

#if DEBUG
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
#endif

            await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}