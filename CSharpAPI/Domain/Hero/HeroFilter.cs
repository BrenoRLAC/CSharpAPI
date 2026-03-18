using API.Domain.Pagination;
using System.Text.Json.Serialization;
namespace API.Domain.Hero
{
    public class HeroFilter : PaginationRequest
    {
        [JsonPropertyName("name")] public string? Search { get; set; }
    

    }
}
