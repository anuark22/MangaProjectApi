using System.Data;

namespace Services.Interfaces
{
    public interface ICommonService
    {
        ICollection<T> Execute<T>(string v, object param, CommandType commandType);
        ICollection<T> Query<T>(string query, object param, CommandType commandType);
        ICollection<T> Query<T>(string query, object param);
        ICollection<T> Query<T>(string v);
        ICollection<T> Query<T>(string v, string splitOn);
        T QueryFirstOrDefault<T>(string query, object parameters = null);
        ICollection<T> Query<T, Q>(string sql, Func<T, Q, T> mapFunc, string splitOn = "id", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);
        ICollection<T> Query<T, Q, Z>(string sql, Func<T, Q, Z, T> mapFunc, string splitOn = "id", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);
        ICollection<T> Query<T, Q, Y, Z>(string sql, Func<T, Q, Y, Z, T> mapFunc, string splitOn = "id", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);
        ICollection<T> Query<T, Q, X, Y, Z>(string sql, Func<T, Q, X, Y, Z, T> mapFunc, string splitOn = "id", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);
        ICollection<T> Query<T, P, Q, X, Y, Z>(string sql, Func<T, P, Q, X, Y, Z, T> mapFunc, string splitOn = "id", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);
        Task<List<T>> QueryAsync<T>(string v, object param);
        Task<List<T>> QueryAsync<T>(string v, object param, CommandType commandType);
        Task<List<T>> QueryAsync<T, Q, X, Y, Z>(string sql, Func<T, Q, X, Y, Z, T> mapFunc, string splitOn = "id", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);
        Task<List<T>> QueryAsync<T, P, Q, X, Y, Z>(string sql, Func<T, P, Q, X, Y, Z, T> mapFunc, string splitOn = "id", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);
        Task<List<T>> QueryAsync<T, Q, X, Y>(string sql, Func<T, Q, X, Y, T> mapFunc, string splitOn = "id", object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);
        Task<TResult> ExecuteWithRetry<TResult>(Func<Task<TResult>> operation, int maxRetries = 3);
    }
}
