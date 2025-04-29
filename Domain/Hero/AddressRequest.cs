

using System.ComponentModel.DataAnnotations;

namespace API.Domain.Hero.AddressRequest
{
    public class AddressRequest
    {

        [MaxLength(400, ErrorMessage = "The max length is {400}")]
        [Required(ErrorMessage = "The field 'street' is required", AllowEmptyStrings = false)]
        public string Street { get; set; }

        [MaxLength(50, ErrorMessage = "The max length is {50}")]
        [Required(ErrorMessage = "The field 'number' is required", AllowEmptyStrings = false)]
        public string Number { get; set; }

        [MaxLength(200, ErrorMessage = "The max length is {200}")]
        public string Complement { get; set; }
       
        [MaxLength(8, ErrorMessage = "The max length is {8}")]
        [Required(ErrorMessage = "The field 'zipCode' is required", AllowEmptyStrings = false)]
        public string ZipCode { get; set; }

        [MaxLength(200, ErrorMessage = "The max length is {200}")]
        public string ReferencePoint { get; set; }
    
        [MaxLength(200, ErrorMessage = "The max length is {200}")]
        [Required(ErrorMessage = "The field 'city' is required", AllowEmptyStrings = false)]
        public string City { get; set; }

        [MaxLength(200, ErrorMessage = "The max length is {200}")]
        [Required(ErrorMessage = "The field 'neighborhood' is required", AllowEmptyStrings = false)]
        public string Neighborhood { get; set; }

        [MaxLength(200, ErrorMessage = "The max length is {200}")]
        [Required(ErrorMessage = "The field 'state' is required", AllowEmptyStrings = false)]
        public string State { get; set; }

        [Required(ErrorMessage = "The field 'country' is required", AllowEmptyStrings = false)]
        public string Country { get; set; } = "BRASIL";

    }
}

