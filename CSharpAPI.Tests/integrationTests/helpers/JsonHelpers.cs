using System.Text.Json;

namespace CSharpAPI.Tests.integrationTests.helpers
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

        public static async Task<T> WaitForValueAsync<T>(Func<T> selector, int timeoutSeconds = 10) where T : class
        {
            var timeout = DateTime.Now.AddSeconds(timeoutSeconds);
            while (DateTime.Now < timeout)
            {
                var value = selector();
                if (value != null) return value;

                await Task.Delay(500);
            }
            throw new TimeoutException($"Background task timed out waiting for {typeof(T).Name}.");

        }
    }
}