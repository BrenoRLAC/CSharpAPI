using API.Domain.Pagination;
using System.Text.Json.Serialization;

namespace API.Domain
{
    public class ReturnApiPaged<T>
    {
        public ReturnApiPaged()
        {
        }

        public ReturnApiPaged(int status, string message)
        {
            StatusCode = status;
            Message = message;
        }

        public ReturnApiPaged(int status, string message, T data)
        {
            StatusCode = status;
            Message = message;
            Data = data;
        }

        public ReturnApiPaged(int status, T data)
        {
            StatusCode = status;
            Data = data;
        }

        public ReturnApiPaged(int status, T data, PaginationResult paginationResult)
        {
            StatusCode = status;
            Data = data;
            PaginationResult = paginationResult;
        }


        [JsonPropertyName("success")] public bool Success => StatusCode is >= 200 and <= 299;

        [JsonPropertyName("statusCode")] public int StatusCode { get; set; }

        [JsonPropertyName("message")] public string Message { get; set; }

        [JsonPropertyName("data")] public T Data { get; set; }

        [JsonPropertyName("pagination")] public PaginationResult PaginationResult { get; set; }

        public bool ShouldSerializeMessage() => !string.IsNullOrEmpty(Message);

        public bool ShouldSerializeData() => Data != null;
        public bool ShouldSerializePaginationResult() => PaginationResult != null;

    }

}