using MimeKit;

namespace API.Infrastructure.Interfaces
{
    public interface IMailer
    {
        Task SendEmailAsync(string name, string email, string subject, string body, MimeEntity mimeEntity = null);
    }
}