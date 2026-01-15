using Microsoft.Data.SqlClient;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Location.Infrastructures.Redis
{
    public interface IRedisConnectionFactory
    {
        IConnectionMultiplexer Create();
    }

    //public class RedisConnectionFactory : IRedisConnectionFactory
    //{
    //    private readonly string _cs;
    //    public RedisConnectionFactory(string cs) => _cs = cs;
    //    public IConnectionMultiplexer Create() => ConnectionMultiplexer.Connect(_cs);
    //}

    public class RedisConnectionFactory : IRedisConnectionFactory
    {
        private readonly Lazy<IConnectionMultiplexer> _lazy;

        public RedisConnectionFactory(string cs)
        {
            _lazy = new Lazy<IConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(cs));
        }

        public IConnectionMultiplexer Create() => _lazy.Value;
    }

}
