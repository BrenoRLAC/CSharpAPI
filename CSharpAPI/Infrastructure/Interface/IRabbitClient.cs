using API.Domain.Notification;

namespace API.Infrastructure.Interface
{
    public interface IRabbitClient
    {
        public void SendMessage(NotificationRequest request);
    }
}
