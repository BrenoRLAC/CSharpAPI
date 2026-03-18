using API.Domain;
using API.Jobs.Interfaces;
using API.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
public class AuthenticationFactory : WebApplicationFactory<Program>
{
    public EmailSpy EmailSpy { get; } = new EmailSpy();

    public void ExpireTokenInCache(string encryptedToken)
    {
        var cache = Services.GetRequiredService<IMemoryCache>();

        if (!encryptedToken.TryDecryptCookie(out string key))
            throw new Exception($"Falha ao descriptografar: {encryptedToken}");

        if (!cache.TryGetValue(key, out _))
            throw new Exception($"Chave '{key}' não encontrada no cache.");

        cache.Remove(key);

        if (cache.TryGetValue(key, out _))
            throw new Exception("Falha crítica: O item ainda persiste no cache.");
    }


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddUserSecrets<AuthenticationFactory>();
            config.AddEnvironmentVariables();
        });

        builder.ConfigureServices(services =>
        {
            var hostedServicesToRemove = services.Where(d =>
                d.ImplementationType != null &&
                (d.ImplementationType.Name.Contains("RabbitListener") ||
                 d.ImplementationType.Name.Contains("Hangfire"))
            ).ToList();

            foreach (var desc in hostedServicesToRemove)
            {
                services.Remove(desc);
            }
            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
            });
            services.AddSingleton<ISendEmail>(EmailSpy);
        });
    }
}


//using API.Jobs.Interfaces;
//using CSharpAPI.Tests.integrationTests.Base;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using NETCore.MailKit.Core;
//public class AuthenticationFactory : WebApplicationFactory<Program>
//{
//    // 1. Create the instance here so the Test can access it
//    public EmailSpy EmailSpy { get; } = new EmailSpy();

//    protected override void ConfigureWebHost(IWebHostBuilder builder)
//    {
//        builder.UseEnvironment("Development");

//        builder.ConfigureServices(services =>
//        {
//            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailService));
//            if (descriptor != null) services.Remove(descriptor);




//        });
//    }
//}