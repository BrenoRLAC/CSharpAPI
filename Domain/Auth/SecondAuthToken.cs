using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Domain.Auth
{
    public class SecondAuthToken
    {
        [Required(ErrorMessage = "Token is required", AllowEmptyStrings = false)]
        [FromForm(Name = "token")]
        public string Token { get; set; }
    }
}