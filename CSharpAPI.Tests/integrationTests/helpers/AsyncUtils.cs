
namespace API.Tests.IntegrationTests.AsyncUtils
{
    public static class AsyncUtils
    {
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