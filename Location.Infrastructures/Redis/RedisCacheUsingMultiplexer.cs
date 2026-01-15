using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Location.Infrastructures.Redis
{
    public interface IRedisCacheUsingMultiplexer
    {
        Task SetAsync<T>(string key, T value, TimeSpan? ttl = null);
        Task<T?> GetAsync<T>(string key);
        Task DeleteAsync(string key);
    }

    public class RedisCacheUsingMultiplexer : IRedisCacheUsingMultiplexer
    {
        private readonly IConnectionMultiplexer _mux;

        public RedisCacheUsingMultiplexer(IRedisConnectionFactory factory)
            => _mux = factory.Create();

        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
        {
            var db = _mux.GetDatabase();
            var json = System.Text.Json.JsonSerializer.Serialize(value);
            await db.StringSetAsync(key, json, ttl);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var db = _mux.GetDatabase();
            var json = await db.StringGetAsync(key);
            return json.HasValue ? System.Text.Json.JsonSerializer.Deserialize<T>(json!) : default;
        }

        public Task DeleteAsync(string key)
        {
            var db = _mux.GetDatabase();
            return db.KeyDeleteAsync(key);
        }
    }
}
