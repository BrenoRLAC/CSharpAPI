using API.Domain.Pagination;
using Newtonsoft.Json;

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


        [JsonProperty("success")] public bool Success => StatusCode is >= 200 and <= 299;

        [JsonProperty("statusCode")] public int StatusCode { get; set; }

        [JsonProperty("message")] public string Message { get; set; }

        [JsonProperty("data")] public T Data { get; set; }

        [JsonProperty("pagination")] public PaginationResult PaginationResult { get; set; }

        public bool ShouldSerializeMessage() => !string.IsNullOrEmpty(Message);

        public bool ShouldSerializeData() => Data != null;
        public bool ShouldSerializePaginationResult() => PaginationResult != null;

    }

}