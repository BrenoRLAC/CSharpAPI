using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Domain.Auth
{
    public class SecondAuthToken
    {
        [Required(ErrorMessage = "Token is required", AllowEmptyStrings = false)]
        [FromForm(Name = "secondAuthToken")]
        public string SecondAuthenticationToken { get; set; }
    }
}