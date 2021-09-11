using System;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Collections.Generic;
using ServiceStack;
using ServiceStack.Redis;

namespace ExemploRedis.Stores.Caching
{
    public interface ICacheService
    {
        T Get<T>(string key) where T : class;
        IEnumerable<T> GetAll<T>(string key) where T : class;
        void Set<T>(string key, T value, TimeSpan time) where T : class;
        bool IsSet(string key);
        void Clear(string key);
        void ClearKeysByPattern(string pattern);
    }

    public class RedisCacheService : ICacheService
    {
        private IConfiguration _configuration;
        private readonly string _connectionString;

        public RedisCacheService(IConfiguration configuration)
        {
            _configuration = configuration;
             _connectionString = _configuration.GetConnectionString("RedisServer");
        }

        public void Set<T>(string key, T value, TimeSpan time) where T : class
        {
            using (IRedisClient client = new RedisClient(new Uri(_connectionString)))
            {
                client.Set(key, value, time);
            }
        }

        public T Get<T>(string key) where T : class
        {
            using (IRedisClient client = new RedisClient(new Uri(_connectionString)))
            {
                return client.Get<T>(key);
            }
        }

        public IEnumerable<T> GetAll<T>(string key) where T : class
        {
            using (IRedisClient client = new RedisClient(new Uri(_connectionString)))
            {
                var keys = GetKeysByPattern($"{key}*");
                var lista = client.GetValues<T>(keys);

                return lista;
            }
        }

        public bool IsSet(string key)
        {
            using (IRedisClient client = new RedisClient(new Uri(_connectionString)))
            {
                return client.ContainsKey(key);
            }
        }

        public void Clear(string key)
        {
            using (IRedisClient client = new RedisClient(new Uri(_connectionString)))
            {
                client.Remove(key);
            }
        }

        public void ClearKeysByPattern(string pattern)
        {
            var keys = GetKeysByPattern(pattern);
            if (keys != null || keys.Count >= 0)
            {
                foreach (var key in keys)
                    Clear(key);
            }
        }

        private List<string> GetKeysByPattern(string pattern)
        {
            using (IRedisClient client = new RedisClient(new Uri(_connectionString)))
            {
                return client.GetKeysByPattern(pattern).ToList();
            }
        }
    }

}
