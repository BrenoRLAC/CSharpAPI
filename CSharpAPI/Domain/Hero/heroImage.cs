
using Newtonsoft.Json;

namespace API.Domain.HeroImages
{
    public class HeroImage
    {
        [JsonProperty("public_id")]
        public string PublicId { get; set; }


        [JsonProperty("secure_url")]
        public string SecureUrl { get; set; }

    }
}

