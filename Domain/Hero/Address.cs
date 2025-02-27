using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace API.Domain.Hero.Addresses
{
    public class Address
    {

        [JsonPropertyName("logradouro")]
        [MaxLength(400, ErrorMessage = "The max length is {1}")]
        [Required(ErrorMessage = "The field 'street' is required", AllowEmptyStrings = false)]
        public string Street { get; set; }

        [JsonPropertyName("numero")]
        [MaxLength(50, ErrorMessage = "The max length is {1}")]
        [Required(ErrorMessage = "The field 'number' is required", AllowEmptyStrings = false)]
        public string Number { get; set; }

        [JsonPropertyName("complemento")]
        [MaxLength(200, ErrorMessage = "The max length is {1}")]
        public string Complement { get; set; }

        [JsonPropertyName("cep")]
        [MaxLength(8, ErrorMessage = "The max length is {1}")]
        [Required(ErrorMessage = "The field 'zipCode' is required", AllowEmptyStrings = false)]
        public string ZipCode { get; set; }

        [JsonPropertyName("pontoDeReferencia")]
        [MaxLength(200, ErrorMessage = "The max length is {1}")]
        public string ReferencePoint { get; set; }

        [JsonPropertyName("localidade")]
        [MaxLength(200, ErrorMessage = "The max length is {1}")]
        [Required(ErrorMessage = "The field 'city' is required", AllowEmptyStrings = false)]
        public string City { get; set; }

        [JsonPropertyName("bairro")]
        [MaxLength(200, ErrorMessage = "The max length is {1}")]
        [Required(ErrorMessage = "The field 'neighborhood' is required", AllowEmptyStrings = false)]
        public string Neighborhood { get; set; }

        [JsonPropertyName("uf")]
        [MaxLength(200, ErrorMessage = "The max length is {1}")]
        [Required(ErrorMessage = "The field 'state' is required", AllowEmptyStrings = false)]
        public string State { get; set; }

        [JsonPropertyName("pais")]
        [Required(ErrorMessage = "The field 'country' is required", AllowEmptyStrings = false)]
        public string Country { get; set; } = "BRASIL";
        

    }
}

