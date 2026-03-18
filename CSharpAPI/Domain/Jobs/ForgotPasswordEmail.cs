namespace API.Domain.Jobs
{
    public class ForgotPasswordEmail
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string TempPassword { get; set; }

    }
}