using API.Infrastructure.Interface;
using Hangfire.Server;
using API.Utilities;

namespace API.Jobs
{
    public class RedisUpdate : IRedisUpdate
    {

        private readonly IHeroDao _heroDao;

        private readonly IRedisDao _redisDao;
        
        private readonly IConfiguration _configuration;


        public RedisUpdate(IHeroDao heroDao, IRedisDao redisDao, IConfiguration configuration)

        {
            _configuration = configuration;
            _heroDao = heroDao;
            _redisDao = redisDao;
            
        }


        public async Task Run(PerformContext context)
        {
            var activeHeroes = await _heroDao.ListActiveHeroes();
            await _redisDao.setAsync(_configuration["Metadata:activeHeroes"], activeHeroes.ToJson());
        }
    }

   


}
