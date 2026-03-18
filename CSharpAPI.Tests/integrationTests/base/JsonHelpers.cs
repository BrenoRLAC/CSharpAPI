using API.Domain;
using API.Domain.Auth;
using System.Net;
using System.Text.Json;

namespace YourProject.Tests.Base
{
    public static class JsonHelpers
    {
        public static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task<T> ReadAsJsonAsync<T>(this HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, DefaultOptions);
        }


        // --- Auth Utility Helpers ---

        public static FormUrlEncodedContent CreateAuthForm(string grantType, string email, string password)
        {
            return new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", grantType),
                new KeyValuePair<string, string>("email", email),
                new KeyValuePair<string, string>("Password", password)
            });
        }

        public static async Task<(string Token, HttpStatusCode Status)> GetInitialAuthToken(HttpClient client, string email, string password)
        {
            var form = CreateAuthForm("password", email, password);
            var resp = await client.PostAsync("/user/Authentication", form);
         
            var body = await resp.ReadAsJsonAsync<ReturnApi<SecondAuthToken>>();
            return (body?.Data?.SecondAuthenticationToken, resp.StatusCode);
        }

        public static async Task<string> GetResetTokenFromLogin(HttpClient client, string email, string password, string grantType)
        {
            var formContent = CreateAuthForm(grantType, email, password);
            var resp = await client.PostAsync("/user/Authentication", formContent);
            resp.EnsureSuccessStatusCode();

            var content = await resp.ReadAsJsonAsync<ReturnApi<LoginResetTemporary>>();
            return content?.Data?.data?.ResetPwdToken;
        }
   

        public static async Task<T> WaitForValueAsync<T>(Func<T> selector, int timeoutSeconds = 10) where T : class
        {
            var timeout = DateTime.Now.AddSeconds(timeoutSeconds);
            while (DateTime.Now < timeout)
            {
                var value = selector();
                if (value != null) return value;

                await Task.Delay(500);
            }
            throw new TimeoutException($"The background task did not provide a value of type {typeof(T).Name} within {timeoutSeconds}s.");
        }
    }
}