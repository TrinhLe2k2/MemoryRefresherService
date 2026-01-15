using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Location.Infrastructures.Redis
{
    public interface IRedisCacheUsingDistributed
    {
        Task SetAsync<T>(string recordId, T data, TimeSpan? absoluteExpireTime = null, TimeSpan? slidingExpireTime = null, bool keepExpiration = false);
        Task<T?> GetAsync<T>(string recordId);
        void DeleteAsync(string pattern);
    }

    public class RedisCacheUsingDistributed : IRedisCacheUsingDistributed
    {
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _mux;

        public RedisCacheUsingDistributed(IDistributedCache cache, IRedisConnectionFactory factory)
        {
            _cache = cache;
            _mux = factory.Create();
        }


        public async Task SetAsync<T>(string recordId, T data, TimeSpan? absoluteExpireTime = null, TimeSpan? slidingExpireTime = null, bool keepExpiration = false)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = keepExpiration ? null : (absoluteExpireTime ?? TimeSpan.FromHours(1)),
                    SlidingExpiration = slidingExpireTime
                };

                var json = JsonSerializer.Serialize(data);
                await _cache.SetStringAsync(recordId, json, options);
            }
            catch (Exception) { throw; }
        }

        public async Task<T?> GetAsync<T>(string recordId)
        {
            try
            {
                var json = await _cache.GetStringAsync(recordId);

                return json is null
                    ? default
                    : JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception) { throw; }
        }

        public void DeleteAsync(string pattern)
        {
            try
            {
                foreach (var endpoint in _mux.GetEndPoints())
                {
                    var server = _mux.GetServer(endpoint);
                    if (!server.IsConnected) continue;

                    var db = _mux.GetDatabase();

                    foreach (var key in server.Keys(pattern: pattern))
                    {
                        db.KeyDelete(key);
                    }
                }
            }
            catch (Exception) { throw; }
        }
    }
}
