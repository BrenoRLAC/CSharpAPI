using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace API.Domain.Hero
{
    public class HeroRequest
    {

        [JsonPropertyName("name")]
        [Required(ErrorMessage = "Campo nome é obrigatório!")]
        [Length(4, 255, ErrorMessage = "Nome deve ter entre {1} e {2} caracteres.")]
        public required string Name { get; set; }

        [JsonPropertyName("disguise")]
        [Required(ErrorMessage = "Campo Nome do disfarce é obrigatório!")]
        [Length(4, 200, ErrorMessage = "Disfarce deve ter entre {1} e {2} caracteres.")]
        public required string Disguise { get; set; }
        [JsonPropertyName("description")]
        [Required(ErrorMessage = "Campo Descrição é obrigatório!")]
        [Length(4, 255, ErrorMessage = "Descrição deve ter entre {1} e {2} caracteres.")]
        public required string Description { get; set; }
        
    }
}
