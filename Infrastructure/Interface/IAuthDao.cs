using API.Domain.Auth;
namespace API.Infrastructure.Interface

{
    public interface IAuthDao
    {
        Task<AuthResult> AccessApi(AuthRequest request);
        Task<AuthResult> AccessApi(ForgotPassword request);
        Task ResetPassword(ResetPasswordRequest request, string username);
        Task<List<PassHist>> PasswordHistory(int codUser);
        Task ForgotPassword(ForgotPassword request, string defaultPass);
        Task GenerateSecondAuth(SecondAuthenticationRequest request);
        Task<AuthResult> SecondAuthentication(SecondAuthenticationRequest request);


    }
}