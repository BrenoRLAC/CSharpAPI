using API.Utilities;
using API.Domain;
using API.Domain.Notification;
using API.Infrastructure.Interface;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace API.Infrastructure
{
    [Authorize]
    public class NotificationHub : Hub, INotificationHub
    {
        public static readonly List<UserSignalR> UsersSocket = new();
        private readonly IHubContext<NotificationHub> _ctx;

        public NotificationHub(IHubContext<NotificationHub> ctx)
        {
            _ctx = ctx;
        }

        public override async Task OnConnectedAsync()
        {
            var context = Context.GetHttpContext();

            var codUser = context.User.Identity.GetCodUser();

            lock (UsersSocket)
            {
                UsersSocket.Add(new UserSignalR
                {
                    DateTime = DateTime.Now,
                    Application = context.Request.Headers["Host"],
                    Environment = context.Request.Headers["Origin"],
                    ConnectionId = Context.ConnectionId,
                    CodUser = codUser.EncryptInt()
                });
            }


            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            lock (UsersSocket)
            {
                var users = UsersSocket.Where(p => p.ConnectionId == Context.ConnectionId).ToList();
                foreach (var user in users)
                    UsersSocket.Remove(user);
            }


            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotification(string codUser, NotificationData message)
        {
            var users = new List<UserSignalR>();
            lock (UsersSocket)
                users.AddRange(UsersSocket.Where(x => x != null && x.CodUser == codUser));

            foreach (var user in users)
                await _ctx.Clients.Client(user.ConnectionId).SendAsync("ReceiverNotification", message.ToJson());
        }

    }
}
