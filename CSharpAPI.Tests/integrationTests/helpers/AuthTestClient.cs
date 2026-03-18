using API.Domain;
using API.Domain.Auth;
using System.Net;
using System.Net.Http.Json;
using CSharpAPI.Tests.integrationTests.Base.BaseIntegrationTest;
using API.Tests.IntegrationTests.AsyncUtils;

namespace CSharpAPI.Tests.integrationTests.helpers
{
    public class AuthTestClient(HttpClient client, EmailSpy emailSpy)
    {
        private readonly HttpClient _client = client;
        private readonly EmailSpy _emailSpy = emailSpy;
        public FormUrlEncodedContent CreateAuthForm(string grantType, string email, string password)
        {
            return new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", grantType),
                new KeyValuePair<string, string>("email", email),
                new KeyValuePair<string, string>("Password", password)
            });
        }

        public async Task<HttpResponseMessage> GetInitialAuthToken(string email, string password, string grantType)
        {
            var form = CreateAuthForm(grantType, email, password);
            return await _client.PostAsync("/user/Authentication", form);
        }


        public async Task<BearerToken> AuthenticatedUser()
        {
            var userName = DataGenerator.GetRandomUserName();
            var email = DataGenerator.GetRandomEmail();
            var password = DataGenerator.GetRandomPassword();

            var regResp = await RegisterNewRandomUser(userName, email, password);
            Assert.Equal(HttpStatusCode.OK, regResp.StatusCode);

            var resetToken = await GetResetTokenFromLogin(email, password);

            var newPassword = "NewSecurePassword123!";
            var resetResp = await ResetPassword(resetToken, newPassword);

            Assert.Equal(HttpStatusCode.OK, resetResp.StatusCode);

            var finalResp = await GetInitialAuthToken(email, newPassword, DataGenerator.GrantType);

            Assert.Equal(HttpStatusCode.OK, finalResp.StatusCode);

            var finalResult = await finalResp.Content.ReadFromJsonAsync<ReturnApi<SecondAuthToken>>();
            Assert.NotNull(finalResult?.Data?.SecondAuthenticationToken);
            Assert.Contains("Second authentication code sent to your email", finalResult.Message);
            
            string code = await AsyncUtils.WaitForValueAsync(() => _emailSpy.GetValueFor(email), 30);
            Assert.NotNull(code);

            var bearerToken = await SendSecondAuthCode(finalResult.Data.SecondAuthenticationToken, code);

            if (!bearerToken.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to authenticate user. Status code: {bearerToken.StatusCode}");
            }

            // 1. Deserialize into the Wrapper first
            var result = await bearerToken.Content.ReadFromJsonAsync<ReturnApi<BearerToken>>();

            // 2. Return the Data part, which contains the actual BearerToken object
            return result?.Data;

        }
        public async Task<string> GetResetTokenFromLogin(string email, string password, string grantType = "password")
        {
            var formContent = CreateAuthForm(grantType, email, password);
            var resp = await _client.PostAsync("/user/Authentication", formContent);
            resp.EnsureSuccessStatusCode();

            var content = await resp.ReadAsJsonAsync<ReturnApi<LoginResetTemporary>>();
            return content?.Data?.data?.ResetPwdToken;
        }

        public async Task<HttpResponseMessage> ResetPassword(string token, string newPassword)
        {
            var resetRequest = new ResetPasswordRequest
            {
                ResetPwdToken = token,
                NewPassword = newPassword,
                RepeatPassword = newPassword
            };

            return await _client.PutAsJsonAsync("/user/Authentication/resetPassword", resetRequest);
        }

        public async Task<HttpResponseMessage> ResetPasswordWrongRepeated(string token, string newPassword)
        {
            var resetRequest = new ResetPasswordRequest
            {
                ResetPwdToken = token,
                NewPassword = newPassword,
                RepeatPassword = "notthesamepassword"
            };

            return await _client.PutAsJsonAsync("/user/Authentication/resetPassword", resetRequest);
        }

        public async Task<HttpResponseMessage> ForgotPassword(string email)
        {
            var forgotReq = new { email = email };
            return await _client.PutAsJsonAsync("/user/Authentication/forgotPassword", forgotReq);
        }

        public async Task<HttpResponseMessage> SendSecondAuthCode(string token, string code)
        {
            var form = new FormUrlEncodedContent([new("secondAuthToken", token)]);
            return await _client.PostAsync($"/user/Authentication/secondAuthentication/{code}", form);
        }

        public async Task<HttpResponseMessage> RegisterNewRandomUser(string userName, string email, string password)
        {
            var request = new UserRequest
            {
                UserName = userName,
                Email = email,
                Password = password
            };
            return await _client.PostAsJsonAsync("/user/Authentication/register", request);
        }
    }
}