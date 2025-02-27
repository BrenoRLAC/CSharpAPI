using API.Domain.Hero.AddressResults;
using API.Domain.HeroImages;
using API.Domain.Pagination;
using Newtonsoft.Json;

namespace API.Domain.Hero
{
    public class HeroFilter : PaginationRequest
    {
        [JsonProperty("name")] public string? Search { get; set; }
    

    }
}
