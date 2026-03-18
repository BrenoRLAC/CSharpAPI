using API.Domain.Auth;
using API.Domain.Jobs;
using API.Jobs.Interfaces;
using Hangfire.Server;
using System.Collections.Concurrent;
public class EmailSpy : ISendEmail
{
    private readonly ConcurrentDictionary<string, string> _store = new();
    public void ResetEmail() => _store.Clear();
    public Task Send(PerformContext ctx, ForgotPasswordEmail e) => Save(e.Email, e.TempPassword);
    public Task Send(PerformContext ctx, SecondAuthenticationEmail e) => Save(e.Email, e.Code);
    private Task Save(string email, string value)
    {
        _store[email] = value;
        return Task.CompletedTask;
    }
    public string GetValueFor(string email) => _store.GetValueOrDefault(email);
}