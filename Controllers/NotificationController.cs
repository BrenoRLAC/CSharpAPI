using API.Domain;
using API.Domain.Notification;
using API.Infrastructure.Interface;
using API.Infrastructure.Interfaces;
using API.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly INotificationHub _hub;
        private readonly INotificationService _service;

        public NotificationController(ILogger<NotificationController> logger, INotificationHub hub,
            INotificationService service)
        {
            _logger = logger;
            _hub = hub;
            _service = service;
        }

        [HttpGet, Produces("application/json", Type = typeof(ReturnApi<NotificationResult>))]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation("[API] Request GET /Notification");

            var codUser = User.Identity.GetCodUser();

            var all = await _service.ListPendingNotification(codUser);

            var result = new NotificationResult
            {
                All = all,
                UnRead = all.Where(x => !x.Read).ToList()
            };

            return Ok(new ReturnApi<NotificationResult>(200, result));
        }

        [HttpPut, Route("Read/{messageId:int}")]
        public async Task<IActionResult> Read(int messageId)
        {
            _logger.LogInformation("[API] Request GET /Notification/Read/{@Request}", messageId);
            await _service.UpdateNotification(messageId, User.Identity.GetCodUser(), true);
            return Ok();
        }

        [HttpPut, Route("Unread/{messageId:int}")]
        public async Task<IActionResult> Unread(int messageId)
        {
            _logger.LogInformation("[API] Request GET /Notification/Unread/{@Request}", messageId);
            await _service.UpdateNotification(messageId, User.Identity.GetCodUser(), false);
            return Ok();
        }

        [HttpPut, Route("Read/All")]
        public async Task<IActionResult> ReadAll(int messageId)
        {
            _logger.LogInformation("[API] Request GET /Notification/ReadAll");
            await _service.ReadAllNotifications(User.Identity.GetCodUser());
            return Ok();
        }

        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("signalr")]
        public async Task<IActionResult> SignalR([FromBody] NotificationDataRequest request)
        {
            var codUser = User.Identity.GetCodUser().EncryptInt();

            NotificationData notificationData =  await _service.SetNotification(codUser, request);

            await _hub.SendNotification(codUser, notificationData);

            return Ok();
        }

        [HttpGet, Route("Read/Exists")]
        public async Task<IActionResult> HasPending()
        {
            var codUser = User.Identity.GetCodUser();
            var hasUnread = await _service.HasUnreadNotification(codUser);
            return Ok(new ReturnApi<int>(200, hasUnread));
        }
    }
}