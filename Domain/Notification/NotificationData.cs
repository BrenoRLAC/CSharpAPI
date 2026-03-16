using System.Text.Json.Serialization;

namespace API.Domain.Notification
{
    public class NotificationData
    {

        [JsonPropertyName("messageId")] public int MessageId { get; set; }
        [JsonPropertyName("title")] public string Title { get; set; }

        [JsonPropertyName("content")] public string ContentMessage { get; set; }

        [JsonPropertyName("category")] public CategoryMessage Category { get; set; }

        [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }

        [JsonPropertyName("read")] public bool Read { get; set; }       

    }
}