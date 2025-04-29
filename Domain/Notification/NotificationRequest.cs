using System.Text.Json.Serialization;

namespace API.Domain.Notification
{
    public class NotificationRequest
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("contentMessage")]
        public string ContentMessage { get; set; }                    
        public List<(string, string)> ReturnUsers { get; set; }
    }
}