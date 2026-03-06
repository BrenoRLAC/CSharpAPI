using API.Domain.Notification;

namespace API.Infrastructure.Interfaces
{
    public interface INotificationDao
    {     
        Task<List<NotificationData>> ListPendingNotification(int codUser);
        Task UpdateNotification(int messageId, int codUser, bool read);
        Task ReadAllNotifications(int codUser);
        Task <NotificationData> SetNotification(string codUser, NotificationDataRequest request);
        Task<int> HasUnreadNotification(int getCodUser);
        
    }
}