


namespace API.Infrastructure.Interface
{
    public interface IRedisDao
    {
        Task setAsync(string key, string value);
    }
}
