using API.Domain.Auth;
using API.Domain.Jobs;

namespace API.Infrastructure.Interface
{
    public interface IAuthService
    {
        Task<AuthResult> AccessApi(AuthRequest request);
        BearerToken Generate(AuthResult result);
        Task ResetPassword(ResetPasswordRequest request, string email);
        Task<bool> PwdRepeated(int codUser, string pwdEncrypted); 
        Task<bool> PwdRepeated(string codAccess, string pwdEncrypted);
        Task<AuthResult> ForgotPassword(ForgotPassword request);
        Task GenerateSecondAuth(SecondAuthenticationRequest request);
        Task<AuthResult> SecondAuthentication(SecondAuthenticationRequest request);    
        Task SignUp(UserRequest request);
    }
}