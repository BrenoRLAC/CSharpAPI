using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API.Domain.Auth
{
    public class AuthRequest
    {       
        [DefaultValue("")]
        [FromForm(Name = "email")]
        [Required(ErrorMessage = "email is required")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        public string Email { get; set; }
        [DefaultValue("")]
        [FromForm(Name = "password")]
        [Required(ErrorMessage = "password is required", AllowEmptyStrings = false)]

        public string Password { get; set; }
        [DefaultValue("")]
        [FromForm(Name = "grant_type")]
        [Required(ErrorMessage = "grant_type is required", AllowEmptyStrings = false)]
        public string GrantType { get; set; }
    }
}