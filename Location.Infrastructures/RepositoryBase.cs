using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Location.Infrastructures
{
    public abstract class RepositoryBase
    {
        private readonly IDbConnectionFactory _factory;

        protected RepositoryBase(IDbConnectionFactory factory)
        {
            _factory = factory;
        }
        protected IDbConnection CreateConnection()
            => _factory.Create();

        protected async Task<IEnumerable<T>> QueryAsync<T>(string storeName, object? param = null, IDbTransaction? transaction = null, int? timeout = null)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<T>(storeName, param, transaction, timeout, CommandType.StoredProcedure);
        }

        protected async Task<T?> QueryFirstOrDefaultAsync<T>(string storeName, object? param = null, IDbTransaction? transaction = null, int? timeout = null)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<T>(storeName, param, transaction, commandTimeout: timeout, commandType: CommandType.StoredProcedure);
        }

        protected async Task<T?> ExecuteScalarAsync<T>(string storeName, object? param = null, IDbTransaction? transaction = null, int? timeout = null)
            where T : IComparable, IConvertible, IEquatable<T>
        {
            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<T>(storeName, param, transaction, commandTimeout: timeout, commandType: CommandType.StoredProcedure);
        }

        protected async Task<int> ExecuteAsync(string storeName, object? param = null, IDbTransaction? transaction = null, int? timeout = null)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteAsync(storeName, param, transaction, commandTimeout: timeout, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Lưu ý: QueryMultiple trả về GridReader => KHÔNG dùng "using var connection" ở đây,
        /// vì sẽ dispose connection trước khi đọc hết các result sets.
        /// Caller phải dispose GridReader (và connection sẽ được đóng theo).
        /// using var grid = await QueryMultipleAsync("sp_test", new { Id = 1 });
        /// var users = await grid.ReadAsync<User>();
        /// var roles = await grid.ReadAsync<Role>();
        /// </summary>
        protected async Task<GridReader> QueryMultipleAsync(string storeName, object? param = null, IDbTransaction? transaction = null, int? timeout = null)
        {
            var connection = CreateConnection();
            try
            {
                // Mặc định Dapper sẽ mở connection nếu đang đóng.
                // Có thể set CommandFlags nếu muốn.
                var reader = await connection.QueryMultipleAsync(storeName, param, transaction, commandTimeout: timeout, commandType: CommandType.StoredProcedure);
                return reader;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }
    }
}
