

using API.Domain.Notification;
using API.Infrastructure;

namespace API.Infrastructure.Interface;


    public interface INotificationHub
    {
    Task SendNotification(string codUser, NotificationData message);  
}
