
using System.Text.Json.Serialization;

namespace API.Domain.Hero.Addresses
{
    public class Address
    {

        [JsonPropertyName("logradouro")]
        public string Street { get; set; }

        [JsonPropertyName("numero")]
        public string Number { get; set; }

        [JsonPropertyName("complemento")]
        public string Complement { get; set; }

        [JsonPropertyName("cep")]
        public string ZipCode { get; set; }

        [JsonPropertyName("pontoDeReferencia")]
        public string ReferencePoint { get; set; }

        [JsonPropertyName("localidade")]
        public string City { get; set; }

        [JsonPropertyName("bairro")]
        public string Neighborhood { get; set; }

        [JsonPropertyName("uf")]
        public string State { get; set; }

        [JsonPropertyName("pais")]
        public string Country { get; set; } = "BRASIL";
        
    }
}

