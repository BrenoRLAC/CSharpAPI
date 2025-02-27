
using API.Domain.HeroImages;

using Newtonsoft.Json;

namespace API.Domain.Hero
{
    public class HeroesResult
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("disguiseName")] public string DisguiseName { get; set; }

        public List<HeroImage> heroImages { get; set; }

        internal string heroImage { get; set; }

    }
}
