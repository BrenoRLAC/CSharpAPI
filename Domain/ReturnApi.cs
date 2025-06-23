using System.Text.Json.Serialization;

namespace API.Domain
{
    public class ReturnApi<T>
    {
        public ReturnApi()
        {
        }

        public ReturnApi(int status, T data)
        {
            StatusCode = status;
            Data = data;
        }

        public ReturnApi(int status, string message)
        {
            StatusCode = status;
            Message = message;
        }

        public ReturnApi(int status, string message, T data)
        {
            StatusCode = status;
            Message = message;
            Data = data;
        }

        [JsonPropertyName("success")]
        public bool Success => StatusCode is >= 200 and <= 299;

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("code")] public string Code { get; set; }

        [JsonPropertyName("message")] public string Message { get; set; }

        [JsonPropertyName("data")] public T Data { get; set; }

    }
}
