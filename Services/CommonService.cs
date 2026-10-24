using Dapper;
using Dapper.Mapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using Services.Interfaces;

namespace Services
{
    public class CommonService : ICommonService
    {
        private readonly IConfiguration _config;
        public CommonService(IConfiguration config)
        {
            _config = config;
        }
        protected SqlConnection connection => new SqlConnection(
            new SqlConnectionStringBuilder(_config.GetConnectionString("connection")).ConnectionString
        );
        public List<TReturn> Query<T1, T2, T3, TReturn>(string sql, Func<T1, T2, T3, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "id", int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var connection = new SqlConnection(new SqlConnectionStringBuilder(_config.GetConnectionString("connection")).ConnectionString))
            {
                connection.Open();
                return connection.Query<T1, T2, T3, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType).ToList();
            }
        }
        public ICollection<T> Execute<T>(string v, object param, CommandType commandType)
        {
            throw new NotImplementedException();
        }
        public ICollection<T> Query<T>(string query)
        {
            return connection.Query<T>(query).ToList();
        }
        public ICollection<T> Query<T>(string v, object param, CommandType commandType)
        {
            return connection.Query<T>(v, param, commandType: commandType).ToList();
        }
        public ICollection<T> Query<T>(string v, object param)
        {
            return connection.Query<T>(v, param).ToList();
        }
        public async Task<List<T>> QueryAsync<T>(string v, object param)
        {
            var result = await connection.QueryAsync<T>(v, param);
            return result.ToList();
        }
        public async Task<List<T>> QueryAsync<T>(string v, object param, CommandType commandType)
        {
            var result = await connection.QueryAsync<T>(v, param, commandType: commandType);
            return result.ToList();
        }
        public async Task<List<T>> QueryAsync<T, P, Q, X, Y, Z>(string sql, Func<T, P, Q, X, Y, Z, T> mapFunc, string splitOn = "id", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var result = await connection.QueryAsync<T, P, Q, X, Y, Z, T>(
                sql,
                mapFunc,
                param,
                transaction,
                buffered,
                splitOn: splitOn,
                commandTimeout: commandTimeout,
                commandType: commandType);
            return result.ToList();
        }
        public async Task<List<T>> QueryAsync<T, Q, X, Y, Z>(string sql, Func<T, Q, X, Y, Z, T> mapFunc, string splitOn = "id", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var result = await connection.QueryAsync<T, Q, X, Y, Z, T>(
                sql,
                mapFunc,
                param,
                transaction,
                buffered,
                splitOn: splitOn,
                commandTimeout: commandTimeout,
                commandType: commandType);
            return result.ToList();
        }
        public async Task<List<T>> QueryAsync<T, Q, X, Y>(string sql, Func<T, Q, X, Y, T> mapFunc, string splitOn = "id", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var result = await connection.QueryAsync<T, Q, X, Y, T>(
                sql,
                mapFunc,
                param,
                transaction,
                buffered,
                splitOn: splitOn,
                commandTimeout: commandTimeout,
                commandType: commandType);
            return result.ToList();
        }
        public ICollection<T> Query<T>(string v, string splitOn)
        {
            return connection.Query<T>(v, splitOn).ToList();

        }
        public ICollection<T> Query<T, Q>(string sql, Func<T, Q, T> mapFunc, string splitOn = "id", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return connection.Query<T, Q, T>(
                sql,
                mapFunc,
                param,
                transaction,
                buffered,
                splitOn: splitOn,
                commandTimeout: commandTimeout,
                commandType: commandType).ToList();
        }
        public ICollection<T> Query<T, Q, Z>(string sql, Func<T, Q, Z, T> mapFunc, string splitOn = "id", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return connection.Query<T, Q, Z, T>(
                sql,
                mapFunc,
                param,
                transaction,
                buffered,
                splitOn: splitOn,
                commandTimeout: commandTimeout,
                commandType: commandType).ToList();
        }
        public ICollection<T> Query<T, Q, Y, Z>(string sql, Func<T, Q, Y, Z, T> mapFunc, string splitOn = "id", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return connection.Query<T, Q, Y, Z, T>(
                sql,
                mapFunc,
                param,
                transaction,
                buffered,
                splitOn: splitOn,
                commandTimeout: commandTimeout,
                commandType: commandType).ToList();
        }
        public ICollection<T> Query<T, Q, X, Y, Z>(string sql, Func<T, Q, X, Y, Z, T> mapFunc, string splitOn = "id", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return connection.Query<T, Q, X, Y, Z, T>(
                sql,
                mapFunc,
                param,
                transaction,
                buffered,
                splitOn: splitOn,
                commandTimeout: commandTimeout,
                commandType: commandType).ToList();
        }
        public ICollection<T> Query<T, P, Q, X, Y, Z>(string sql, Func<T, P, Q, X, Y, Z, T> mapFunc, string splitOn = "id", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return connection.Query<T, P, Q, X, Y, Z, T>(
                sql,
                mapFunc,
                param,
                transaction,
                buffered,
                splitOn: splitOn,
                commandTimeout: commandTimeout,
                commandType: commandType).ToList();
        }

        public async Task<TResult> ExecuteWithRetry<TResult>(Func<Task<TResult>> operation, int maxRetries = 3)
        {
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    return await operation(); // Intentar ejecutar la operación.
                }
                catch (SqlException ex) when (ex.Number == 1205) // Código de deadlock.
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                        throw; // Si se supera el número de intentos, lanzar la excepción.
                }
            }
            throw new InvalidOperationException("Unreachable code."); // Este punto nunca debería alcanzarse.
        }

        public T QueryFirstOrDefault<T>(string query, object parameters = null)
        {
            return connection.QueryFirstOrDefault<T>(query, parameters);
        }
    }
}
