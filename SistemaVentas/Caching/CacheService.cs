using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SistemaVentas.Caching
{
    /// <summary>
    /// Interfaz de caché que permite intercambiar la implementación
    /// entre memoria local y Redis sin cambiar los consumidores.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>Obtiene un valor del caché.</summary>
        Task<(bool found, T? value)> GetAsync<T>(string key);

        /// <summary>Almacena un valor en el caché con TTL opcional.</summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

        /// <summary>Elimina una clave del caché.</summary>
        Task RemoveAsync(string key);

        /// <summary>Elimina todas las claves que coinciden con el prefijo.</summary>
        Task RemoveByPrefixAsync(string prefix);

        /// <summary>Obtiene o crea un valor (get-or-add pattern).</summary>
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);

        /// <summary>Invalida todo el caché.</summary>
        Task FlushAsync();
    }

    /// <summary>
    /// Implementación de caché en memoria con expiración por TTL.
    /// Para producción con múltiples nodos, reemplazar con RedisCacheService.
    /// Thread-safe mediante ConcurrentDictionary.
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
        private readonly Timer _cleanupTimer;

        public MemoryCacheService()
        {
            // Limpieza periódica de entradas expiradas cada 5 minutos
            _cleanupTimer = new Timer(
                _ => RemoveExpiredEntries(),
                null,
                TimeSpan.FromMinutes(5),
                TimeSpan.FromMinutes(5));
        }

        public Task<(bool found, T? value)> GetAsync<T>(string key)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (!entry.IsExpired)
                {
                    try
                    {
                        var value = (T?)entry.Value;
                        return Task.FromResult((true, value));
                    }
                    catch
                    {
                        _cache.TryRemove(key, out _);
                    }
                }
                else
                {
                    _cache.TryRemove(key, out _);
                }
            }

            return Task.FromResult((false, default(T)));
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var expiresAt = expiry.HasValue
                ? DateTimeOffset.UtcNow.Add(expiry.Value)
                : DateTimeOffset.MaxValue;

            _cache[key] = new CacheEntry(value, expiresAt);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _cache.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        public Task RemoveByPrefixAsync(string prefix)
        {
            foreach (var key in _cache.Keys)
            {
                if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    _cache.TryRemove(key, out _);
            }
            return Task.CompletedTask;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            var (found, value) = await GetAsync<T>(key);
            if (found && value is not null)
                return value;

            var newValue = await factory();
            await SetAsync(key, newValue, expiry);
            return newValue;
        }

        public Task FlushAsync()
        {
            _cache.Clear();
            return Task.CompletedTask;
        }

        private void RemoveExpiredEntries()
        {
            foreach (var kvp in _cache)
            {
                if (kvp.Value.IsExpired)
                    _cache.TryRemove(kvp.Key, out _);
            }
        }

        private sealed class CacheEntry
        {
            public object? Value { get; }
            public DateTimeOffset ExpiresAt { get; }
            public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;

            public CacheEntry(object? value, DateTimeOffset expiresAt)
            {
                Value = value;
                ExpiresAt = expiresAt;
            }
        }
    }
}
