using ExemploRedis.Stores.Caching;
using Microsoft.AspNetCore.Mvc;

namespace ExemploRedis.Controllers
{
    [ApiController]
    [Route("CacheManager")]
    public class CacheManagerController : ControllerBase
    {
        private readonly ICacheService redisService;

        public CacheManagerController(ICacheService redisService)
        {
            this.redisService = redisService;
        }

        [HttpDelete]
        public void Clear()
        {
            redisService.ClearKeysByPattern("*");
        }
    }
}
