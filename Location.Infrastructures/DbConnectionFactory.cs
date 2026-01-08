using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Location.Infrastructures
{
    public interface IDbConnectionFactory
    {
        IDbConnection Create();
    }
    public sealed class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _cs;
        public SqlConnectionFactory(string cs) => _cs = cs;
        public IDbConnection Create() => new SqlConnection(_cs);
    }

}
