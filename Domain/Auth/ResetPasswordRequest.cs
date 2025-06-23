using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace API.Domain.Auth
{
    public class ResetPasswordRequest
    {
        [JsonIgnore] public int CodUser { get; set; }

        [Required(ErrorMessage = "Password is required", AllowEmptyStrings = false)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "NewPassword is required", AllowEmptyStrings = false), Compare("NewPassword")]
        public string RepeatPassword { get; set; }

        [Required(ErrorMessage = "TempToken is required", AllowEmptyStrings = false)]
        public string ResetPwdToken { get; set; }
    }
}