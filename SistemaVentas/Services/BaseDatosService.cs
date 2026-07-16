using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Abstracción mínima de base de datos para mantener compatibilidad en esta fase.
    /// </summary>
    public class BaseDatosService
    {
        public Task InicializarBaseDatosAsync() => Task.CompletedTask;

        public Task<object> ExecuteScalarAsync(string sql, Dictionary<string, object> parametros = null)
            => Task.FromResult<object>(0);

        public Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object> parametros = null)
            => Task.FromResult(0);

        public Task<bool> VerificarIntegridadAsync() => Task.FromResult(true);

        public Task OptimizarAsync() => Task.CompletedTask;
    }
}
