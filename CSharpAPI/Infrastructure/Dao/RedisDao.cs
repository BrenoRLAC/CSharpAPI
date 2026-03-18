using API.Infrastructure.Interface;
using StackExchange.Redis;

namespace API.Infrastructure.Dao
{
    public class RedisDao : IRedisDao
    {

        private IDatabase _database;
        private readonly ConnectionMultiplexer _connection;
        private readonly ILogger<RedisDao> _logger;

        private IDatabase Database => _database ??= _connection.GetDatabase();

        public RedisDao(ConnectionMultiplexer connection, ILogger<RedisDao> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task setAsync(string key, string value)
        {

            _logger.LogInformation("[REDIS] SetAsync {key}", key);
            await Database.StringSetAsync(key, value);
        }
    }
}
