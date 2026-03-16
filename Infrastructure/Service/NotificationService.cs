using API.Domain.Notification;
using API.Infrastructure.Interfaces;
using API.Utilities;

namespace API.Infrastructure.Service
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationDao _dao;

        public NotificationService(INotificationDao dao, IConfiguration configuration,
            ILogger<NotificationService> logger)
        {
            _dao = dao;
        }
        public async Task<List<NotificationData>> ListPendingNotification(int codUser)
        {
            return await _dao.ListPendingNotification(codUser);
        }

        public Task UpdateNotification(int messageId, int codUser, bool read)
        {
            return _dao.UpdateNotification(messageId, codUser, read);
        }

        public Task ReadAllNotifications(int codUser)
        {
            return _dao.ReadAllNotifications(codUser);
        }
        public async Task <NotificationData> SetNotification(string codUser, NotificationDataRequest request)
        {
             return await _dao.SetNotification(codUser, request);
        }

        public async Task<int> HasUnreadNotification(int getCodUser)
        {
            return await _dao.HasUnreadNotification(getCodUser);
        }

    }
}