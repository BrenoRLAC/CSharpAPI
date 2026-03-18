using System.Text.Json.Serialization;

namespace API.Domain.Notification
{
    public class NotificationDataRequest
    {

        [JsonPropertyName("title")] public string Title { get; set; }

        [JsonPropertyName("content")] public string ContentMessage { get; set; }

        [JsonPropertyName("category")] public CategoryMessage Category { get; set; }       

    }
}