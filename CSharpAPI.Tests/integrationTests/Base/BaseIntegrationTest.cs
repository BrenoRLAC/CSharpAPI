using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;



using CSharpAPI.Tests.integrationTests.helpers;

namespace CSharpAPI.Tests.integrationTests.Base.BaseIntegrationTest
        {
    public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestFactory>
                {
        protected readonly HttpClient Client;
        protected readonly IConfiguration Configuration;
        protected readonly AuthTestClient Auth;
        protected readonly DatabaseTestAuthHelper DbAuth;

        protected BaseIntegrationTest(IntegrationTestFactory factory)
        {
            Client = factory.CreateClient();
            Configuration = factory.Services.GetRequiredService<IConfiguration>();
            Configuration = factory.Services.GetRequiredService<IConfiguration>();
            Auth = new AuthTestClient(Client, factory.EmailSpy);
            DbAuth = new DatabaseTestAuthHelper(Configuration);

    }
}
}