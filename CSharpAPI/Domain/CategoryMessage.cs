using System.Text.Json.Serialization;

namespace API.Domain
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CategoryMessage
    {
        success = 1,
        error = 2,
        warning = 3
    }
}