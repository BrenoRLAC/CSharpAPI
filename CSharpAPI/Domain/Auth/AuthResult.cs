namespace API.Domain.Auth
{
    public class AuthResult
    {
        public int CodUser { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool PasswordExpired { get; set; }
        public bool Temporary { get; set; }
        public bool TemporaryPasswordExpired { get; set; }
    }
}