using Hangfire.Server;

public interface IRedisUpdate
{
    Task Run(PerformContext context);
}