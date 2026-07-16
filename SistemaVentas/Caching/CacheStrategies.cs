using System;
using System.Threading.Tasks;

namespace SistemaVentas.Caching
{
    /// <summary>
    /// Estrategias de invalidación de caché organizadas por dominio.
    /// Centraliza la lógica de cuándo y qué invalidar.
    /// </summary>
    public class CacheStrategies
    {
        private readonly ICacheService _cache;

        public CacheStrategies(ICacheService cache)
        {
            _cache = cache;
        }

        // ── Ventas ────────────────────────────────────────────────────────────

        /// <summary>Invalida el caché de ventas cuando se crea o cancela una venta.</summary>
        public async Task InvalidarVentasAsync(DateTime fecha, long usuarioId)
        {
            await _cache.RemoveAsync(CacheKeys.Format(CacheKeys.TotalVentasFecha, fecha.ToString("yyyy-MM-dd")));
            await _cache.RemoveAsync(CacheKeys.Format(CacheKeys.VentasPorFecha,   fecha.ToString("yyyy-MM-dd")));
            await _cache.RemoveAsync(CacheKeys.Format(CacheKeys.VentasPorUsuario, usuarioId));
            await _cache.RemoveAsync(CacheKeys.TotalVentasHoy);
        }

        // ── Caja ─────────────────────────────────────────────────────────────

        /// <summary>Invalida el caché de caja cuando hay un movimiento.</summary>
        public Task InvalidarCajaAsync()
            => _cache.RemoveAsync(CacheKeys.SaldoCaja);

        // ── Sorteos ───────────────────────────────────────────────────────────

        /// <summary>Invalida el caché de sorteos cuando cambia su estado.</summary>
        public async Task InvalidarSorteosAsync(DateTime fecha)
        {
            await _cache.RemoveAsync(CacheKeys.Format(CacheKeys.SorteosDia, fecha.ToString("yyyy-MM-dd")));
            await _cache.RemoveAsync(CacheKeys.SorteosActivos);
        }

        // ── Usuarios ──────────────────────────────────────────────────────────

        /// <summary>Invalida el caché de un usuario específico.</summary>
        public async Task InvalidarUsuarioAsync(long usuarioId, string numero)
        {
            await _cache.RemoveAsync(CacheKeys.Format(CacheKeys.UsuarioPorId,     usuarioId));
            await _cache.RemoveAsync(CacheKeys.Format(CacheKeys.UsuarioPorNumero, numero));
            await _cache.RemoveAsync(CacheKeys.UsuariosActivos);
        }

        // ── Reportes ──────────────────────────────────────────────────────────

        /// <summary>Invalida reportes que incluyan el rango afectado por una venta.</summary>
        public Task InvalidarReportesAsync()
            => _cache.RemoveByPrefixAsync("reportes:");

        // ── Configuración ─────────────────────────────────────────────────────

        /// <summary>Invalida la configuración en caché.</summary>
        public Task InvalidarConfiguracionAsync()
            => _cache.RemoveByPrefixAsync("config:");

        // ── Notificaciones ────────────────────────────────────────────────────

        /// <summary>Invalida el conteo de notificaciones no leídas.</summary>
        public Task InvalidarNotificacionesAsync()
            => _cache.RemoveAsync(CacheKeys.NotificacionesConteo);

        // ── Global ────────────────────────────────────────────────────────────

        /// <summary>Invalida todo el caché (usar con precaución).</summary>
        public Task InvalidarTodoAsync()
            => _cache.FlushAsync();

        // ── Helpers de TTL ────────────────────────────────────────────────────

        public static TimeSpan TtlCorto    => TimeSpan.FromSeconds(CacheKeys.TtlCorto);
        public static TimeSpan TtlMedio    => TimeSpan.FromSeconds(CacheKeys.TtlMedio);
        public static TimeSpan TtlLargo    => TimeSpan.FromSeconds(CacheKeys.TtlLargo);
        public static TimeSpan TtlMuyLargo => TimeSpan.FromSeconds(CacheKeys.TtlMuyLargo);
    }
}
