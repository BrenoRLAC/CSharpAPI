using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace API.Domain.Hero
{
    public class HeroRequest
    {  

        [JsonPropertyName("name")]       
        [Required(ErrorMessage = "Campo nome é obrigatório!")]
        public required string Name { get; set; }

        [JsonPropertyName("disguise")]
        [Required(ErrorMessage = "Campo Nome do disfarce é obrigatório!")]
        public required string Disguise { get; set; }
        [JsonPropertyName("description")]
        [Required(ErrorMessage = "Campo Descrição é obrigatório!")]
        public required string Description { get; set; }
      
    }
}
