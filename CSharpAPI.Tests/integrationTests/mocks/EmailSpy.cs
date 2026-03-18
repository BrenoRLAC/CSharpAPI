using API.Domain.Auth;
using API.Domain.Jobs;
using API.Jobs.Interfaces;
using Hangfire.Server;
using NETCore.MailKit.Core;

public class EmailSpy : ISendEmail
{
    // Use volatile or lock if running tests in parallel
    public string LastSentPassword { get; private set; }
    public string SecondAuthCode { get; private set; }

    public void ResetEmail()
    {
        LastSentPassword = null;
    }

    public Task Send(PerformContext context, ForgotPasswordEmail forgotPassword)
    {
        LastSentPassword = forgotPassword.TempPassword;
        return Task.CompletedTask;
    }

    public Task Send(PerformContext context, SecondAuthenticationEmail forgotPassword)
    {
        SecondAuthCode = forgotPassword.Code;
        return Task.CompletedTask;
    }

  
}