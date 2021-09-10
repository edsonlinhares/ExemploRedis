using System.Linq;
using StackExchange.Redis;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ExemploRedis.Stores.Caching
{
    public static class RedisDataBaseExtensions
    {
        public static async Task<List<T>> Listar<T>(this IDatabase cache, string key)
        {
            var items = await cache.ListRangeAsync(key);

            var lista = items.Select(x => JsonConvert.DeserializeObject<T>(x));

            return lista.ToList();
        }

    }
}
