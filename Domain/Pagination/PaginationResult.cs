using Newtonsoft.Json;

namespace API.Domain.Pagination
{
    public class PaginationResult: Pagination
    {
        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }

        [JsonProperty("totalReg")]
        public int TotalReg { get; set; }

        [JsonProperty("nextPage")]
        public string NextPage { get; set; }
        
        [JsonProperty("prevPage")]
        public string PrevPage { get; set; }
    }
}