using System.Text.Json.Serialization;
namespace API.Domain.Notification
{
    public class NotificationResult
    {
        [JsonPropertyName("all")]
        public List<NotificationData> All { get; set; }

        [JsonPropertyName("unread")]
        public List<NotificationData> UnRead { get; set; }
    }
}