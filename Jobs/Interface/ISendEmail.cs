using API.Domain.Auth;
using API.Domain.Jobs;
using Hangfire.Server;

namespace API.Jobs.Interfaces
{
    public interface ISendEmail
    {
        Task Send(PerformContext context, ForgotPasswordEmail forgotPassword);
        Task Send(PerformContext context, SecondAuthenticationEmail forgotPassword);
    }
}