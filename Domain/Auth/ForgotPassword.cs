using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API.Domain.Auth
{
    public class ForgotPassword
    {
        [Required(ErrorMessage = "email is required", AllowEmptyStrings = false)]      
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        [DefaultValue("")]
        public string Email { get; set; }
     
    }
}