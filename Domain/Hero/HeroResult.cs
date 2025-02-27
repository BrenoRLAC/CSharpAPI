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
        [JsonProperty("disguiseName")] public string DisguiseName { get; set; }
        public List<HeroImage> HeroImages { get; set; }
        internal string HeroImage { get; set; }
        public AddressResult AddressResult { get; set; }

    }
}
