using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace API.Domain.Pagination
{
    public class PaginationResult: Pagination
    {
        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("totalReg")]
        public int TotalReg { get; set; }

        [JsonPropertyName("nextPage")]
        public string NextPage { get; set; }
        
        [JsonPropertyName("prevPage")]
        public string PrevPage { get; set; }
    }
}