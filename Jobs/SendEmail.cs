using API.Constants;
using API.Domain.Auth;
using API.Domain.Jobs;
using API.Infrastructure.Interfaces;
using API.Jobs.Interfaces;
using Hangfire.Server;
using MimeKit;

namespace API.Jobs
{
    public class SendEmail : ISendEmail
    {
        private readonly ILogger<SendEmail> _logger;
        private readonly IMailer _mailer;

        public SendEmail(IMailer mailer, ILogger<SendEmail> logger)
        {
            _mailer = mailer;
            _logger = logger;
        }

        public async Task Send(PerformContext context, ForgotPasswordEmail data)
        {
            _logger.LogInformation("[SERVICE] SendEmail.Send ForgotPasswordEmail {@Data}", data);

            string subject = "Senha temporária";

            string htmlBody = string.Format(EmailTemplate.ForgotPassword, data.Name, data.Email, data.TempPassword);

            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody

            };

            await SendingAttempt(context, data.Name, data.Email, subject, builder.TextBody, builder.ToMessageBody());
        }

        public async Task Send(PerformContext context, SecondAuthenticationEmail data)
        {
            _logger.LogInformation("[SERVICE] SendEmail.Send SecondAuthenticationEmail {@Data}", data);

            string subject = "Código Temporário";

            string htmlBody = string.Format(EmailTemplate.SecondAuth, data.Name, data.Email, data.Code);

            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody

            };

            await SendingAttempt(context, data.Name, data.Email, subject, builder.TextBody, builder.ToMessageBody());
        }


        private async Task SendingAttempt(PerformContext context, string name, string email, string subject, string message, MimeEntity html)
        {
            var retryCount = 0;
            var process = true;
            var delayMax = TimeSpan.FromSeconds(15);

            while (process && retryCount < 3)
            {
                if (retryCount > 0) await Task.Delay(delayMax);

                try
                {
                    await _mailer.SendEmailAsync(name, email, subject, message, html);
                    process = false;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to send email to {Email} with subject '{Subject}' on attempt {Attempt}. Error: {ErrorMessage}",
                    email, subject, retryCount, e.Message);
                }
                retryCount++;
            }
        }
    }
}