using Hangfire.Dashboard;

namespace API.Domain.Auth
{
    public class HangfireAuth : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}