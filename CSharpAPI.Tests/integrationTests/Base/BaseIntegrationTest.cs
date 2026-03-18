using API.Domain;
using API.Domain.Auth;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Net;
using System.Net.Http.Json;

//namespace CSharpAPI.Tests.integrationTests.Base.BaseIntegrationTest
//{
//    public abstract class BaseIntegrationTest(AuthenticationFactory factory)
//        : IClassFixture<AuthenticationFactory>
//    {
//        protected readonly HttpClient Client = factory.CreateClient();
//        protected readonly IConfiguration Configuration = factory.Services.GetRequiredService<IConfiguration>();


//        protected string Email => Configuration["IntegrationTests:ValidUser:Email"]
//            ?? throw new Exception("Email missing in Configuration/Secrets");

//        protected string Password => Configuration["IntegrationTests:ValidUser:Password"]
//            ?? throw new Exception("Password missing in Configuration/Secrets");

//        protected const string GrantType = "password";

//        //Wrong Authentication Informations
//        protected string WrongEmail => Configuration["IntegrationTests:WrongUser:Email"]
//         ?? throw new Exception("Email missing in Configuration/Secrets");

//        protected string WrongPassword => Configuration["IntegrationTests:WrongUser:Password"]
//            ?? throw new Exception("Password missing in Configuration/Secrets");

//        protected const string InvalidGrantType = "something";

//        // Define the ConnectionString property
//        protected string ConnectionString => Configuration.GetConnectionString("Default")
//            ?? throw new Exception("Connection string 'DefaultConnection' not found in configuration.");

//        protected IDbConnection CreateConnection() => new SqlConnection(ConnectionString);

//        // --- Auth Helper Methods ---


//        protected async Task<HttpResponseMessage> ResetPassword(string token, string newPassword)
//        {
//            var resetRequest = new ResetPasswordRequest
//            {
//                ResetPwdToken = token,
//                NewPassword = newPassword,
//                RepeatPassword = newPassword
//            };

//            return await Client.PutAsJsonAsync("/user/Authentication/resetPassword", resetRequest);
//        }

//        protected async Task<HttpResponseMessage> ResetPasswordWrongRepeated(string token, string newPassword)
//        {
//            var resetRequest = new ResetPasswordRequest
//            {
//                ResetPwdToken = token,
//                NewPassword = newPassword,
//                RepeatPassword = "notthesamepassword"
//            };

//            return await Client.PutAsJsonAsync("/user/Authentication/resetPassword", resetRequest);
//        }

//        protected async Task<HttpResponseMessage> ForgotPassword(string email)
//        {
//            var forgotReq = new { email = email };
//            return await Client.PutAsJsonAsync("/user/Authentication/forgotPassword", forgotReq);
//        }




//        protected async Task<HttpResponseMessage> GetInitialAuthToken(string email, string password, string grantType)
//        {
//            var form = CreateAuthForm(grantType, email, password);           
//            return await Client.PostAsync("/user/Authentication", form);
//        }

//        protected async Task<HttpResponseMessage> SendSecondAuthCode(string token,string code)
//        {
//            var form = new FormUrlEncodedContent([new("secondAuthToken", token)]);

//            return await Client.PostAsync($"/user/Authentication/secondAuthentication/{code}", form);

//        }

//        protected async Task<string> GetResetTokenFromLogin(string email, string password)
//        {
//            var formContent = CreateAuthForm(GrantType, email, password);
//            var resp = await Client.PostAsync("/user/Authentication", formContent);
//            resp.EnsureSuccessStatusCode();

//            var content = await resp.Content.ReadFromJsonAsync<ReturnApi<LoginResetTemporary>>();
//            return content?.Data?.data?.ResetPwdToken;
//        }
//        protected FormUrlEncodedContent CreateAuthForm(string grantType, string email, string password)
//        {
//            return new FormUrlEncodedContent([
//                new("grant_type", grantType),
//                new("email", email),
//                new("Password", password)
//            ]);
//        }

//        // --- Utility Methods ---

//        protected async Task<T> WaitForValueAsync<T>(Func<T> selector, int timeoutSeconds = 10) where T : class
//        {

//            var timeout = DateTime.Now.AddSeconds(timeoutSeconds);
//            while (DateTime.Now < timeout)
//            {
//                var value = selector();
//                if (value != null) return value;

//                await Task.Delay(500);
//            }
//            throw new TimeoutException($"Background task timed out waiting for {typeof(T).Name}.");

//        }

//        protected async Task<HttpResponseMessage> RegisterNewRandomUser(string userName, string email, string password)
//        {
//            var request = new UserRequest
//            {
//                UserName = userName,
//                Email = email,
//                Password = password
//            };

//            return await Client.PostAsJsonAsync("/user/Authentication/register", request);
//        }




//        protected async Task<(string Email, string UserName)> GetExistingActiveUser()
//        {
//            using var conn = CreateConnection();           
//            var result = await conn.QueryFirstOrDefaultAsync<(string Email, string UserName)>(@"
//        SELECT TOP 1 
//            EMAIL, 
//            NAME AS UserName
//        FROM USERS
//        WHERE ACTIVE = 1 
//        ORDER BY COD_USER DESC");

//            return result;
//        }
//        protected async Task SetUserPasswordExpirationAsync(string email, DateTime expirationDate, bool isTemporary = false)
//        {
//            using var conn = CreateConnection();
//            await conn.ExecuteAsync(@"
//        WITH LatestPassword AS (
//            SELECT TOP 1 up.CREATED_AT, up.TEMPORARY
//            FROM USER_PASSWORD up
//            INNER JOIN USERS u ON up.COD_USER = u.COD_USER
//            WHERE u.EMAIL = @Email AND up.ACTIVE = 1
//            ORDER BY up.CREATED_AT DESC
//        )
//        UPDATE LatestPassword 
//        SET CREATED_AT = @OldDate, 
//            TEMPORARY = @IsTemp;",
//                new
//                {
//                    OldDate = expirationDate,
//                    Email = email,
//                    IsTemp = isTemporary ? 1 : 0
//                });
//        }


//    }
//}

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