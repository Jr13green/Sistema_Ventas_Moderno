using System.Data;
using Dapper;

namespace SistemaVentas.Database
{
    /// <summary>Generic Dapper repository with connection pooling support.</summary>
    public sealed class DapperRepository
    {
        private readonly ConnectionPool _pool;

        public DapperRepository(ConnectionPool pool)
        {
            _pool = pool;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null,
            CancellationToken ct = default)
        {
            var conn = await _pool.AcquireAsync(ct);
            try
            {
                return await conn.QueryAsync<T>(new CommandDefinition(sql, param,
                    cancellationToken: ct));
            }
            finally
            {
                _pool.Release(conn);
            }
        }

        public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null,
            CancellationToken ct = default)
        {
            var conn = await _pool.AcquireAsync(ct);
            try
            {
                return await conn.QueryFirstOrDefaultAsync<T>(new CommandDefinition(sql, param,
                    cancellationToken: ct));
            }
            finally
            {
                _pool.Release(conn);
            }
        }

        public async Task<int> ExecuteAsync(string sql, object? param = null,
            CancellationToken ct = default)
        {
            var conn = await _pool.AcquireAsync(ct);
            try
            {
                return await conn.ExecuteAsync(new CommandDefinition(sql, param,
                    cancellationToken: ct));
            }
            finally
            {
                _pool.Release(conn);
            }
        }

        public async Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null,
            CancellationToken ct = default)
        {
            var conn = await _pool.AcquireAsync(ct);
            try
            {
                return await conn.ExecuteScalarAsync<T>(new CommandDefinition(sql, param,
                    cancellationToken: ct));
            }
            finally
            {
                _pool.Release(conn);
            }
        }

        /// <summary>Batch insert using Dapper.</summary>
        public async Task<int> BulkInsertAsync<T>(string sql, IEnumerable<T> items,
            CancellationToken ct = default)
        {
            var conn = await _pool.AcquireAsync(ct);
            try
            {
                using var tx = conn.BeginTransaction();
                try
                {
                    var affected = await conn.ExecuteAsync(sql, items, tx);
                    tx.Commit();
                    return affected;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            finally
            {
                _pool.Release(conn);
            }
        }
    }
}
