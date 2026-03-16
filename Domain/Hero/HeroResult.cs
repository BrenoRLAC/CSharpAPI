using API.Domain.Hero.AddressResults;
using API.Domain.HeroImages;
using Newtonsoft.Json;

namespace API.Domain.Hero
{
    public class HeroResult
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("disguise")] public string Disguise { get; set; }
        public List<HeroImage> Images { get; set; }
        internal string Image { get; set; }
        public AddressResult AddressResult { get; set; }

    }
}
