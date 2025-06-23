using Microsoft.AspNetCore.Mvc;

namespace API.Domain.Auth
{
    public class SecondAuthenticationRequest
    {
        public string CodUser { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Code { get; set; }

    }
}