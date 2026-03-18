using System.Text.Json.Serialization;
namespace API.Domain.Auth
{
    public class BearerToken
    {
        [JsonPropertyName("type")]
        public string TokenType { get; set; }

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime Created { get; set; }

        [JsonPropertyName("expiresIn")]
        public DateTime ExpireIn { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; }
    }

}