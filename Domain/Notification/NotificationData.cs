using System.Text.Json.Serialization;

namespace API.Domain.Notification
{
    public class NotificationData
    {
        [JsonPropertyName("messageId")] public int MessageId { get; set; }
        [JsonPropertyName("title")] public string Title { get; set; }
        [JsonPropertyName("content")] public string Content { get; set; }                
        [JsonPropertyName("read")] public bool Read { get; set; }
    
    }
}