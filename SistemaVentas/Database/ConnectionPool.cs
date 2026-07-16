using System.Data;
using Microsoft.Data.Sqlite;

namespace SistemaVentas.Database
{
    /// <summary>Connection pool manager for SQLite/Dapper.</summary>
    public sealed class ConnectionPool : IDisposable
    {
        private readonly string _connectionString;
        private readonly SemaphoreSlim _semaphore;
        private readonly Stack<SqliteConnection> _pool = new();
        private readonly int _maxPoolSize;
        private bool _disposed;

        public ConnectionPool(string connectionString, int maxPoolSize = 10)
        {
            _connectionString = connectionString;
            _maxPoolSize      = maxPoolSize;
            _semaphore        = new SemaphoreSlim(maxPoolSize, maxPoolSize);
        }

        public async Task<IDbConnection> AcquireAsync(CancellationToken ct = default)
        {
            await _semaphore.WaitAsync(ct);
            lock (_pool)
            {
                if (_pool.Count > 0)
                {
                    var conn = _pool.Pop();
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                    return conn;
                }
            }

            var newConn = new SqliteConnection(_connectionString);
            newConn.Open();
            return newConn;
        }

        public void Release(IDbConnection connection)
        {
            if (connection is SqliteConnection sqliteConn)
            {
                lock (_pool)
                {
                    if (_pool.Count < _maxPoolSize)
                    {
                        _pool.Push(sqliteConn);
                        _semaphore.Release();
                        return;
                    }
                }
            }

            connection.Dispose();
            _semaphore.Release();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            lock (_pool)
            {
                while (_pool.Count > 0)
                    _pool.Pop().Dispose();
            }

            _semaphore.Dispose();
        }
    }
}
