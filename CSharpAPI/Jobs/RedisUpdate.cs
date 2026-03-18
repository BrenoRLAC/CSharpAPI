using API.Infrastructure.Interface;
using Hangfire.Server;
using API.Utilities;

namespace API.Jobs
{
    public class RedisUpdate(IHeroDao heroDao, IRedisDao redisDao, IConfiguration configuration) : IRedisUpdate
    {

        private readonly IHeroDao _heroDao = heroDao;
        private readonly IRedisDao _redisDao = redisDao;      
        private readonly IConfiguration _configuration = configuration;

        public async Task Run(PerformContext context)
        {
            var activeHeroes = await _heroDao.ListActiveHeroes();
            await _redisDao.setAsync(_configuration["Metadata:activeHeroes"], activeHeroes.ToJson());
        }
    }

   


}
