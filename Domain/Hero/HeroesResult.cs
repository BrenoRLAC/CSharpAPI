
using API.Domain.HeroImages;

using System.Text.Json.Serialization;

namespace API.Domain.Hero
{
    public class HeroesResult
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("description")] public string Description { get; set; }
        [JsonPropertyName("disguise")] public string Disguise { get; set; }

        public List<HeroImage> Images { get; set; }

        internal string Image { get; set; }

    }
}
