using System;
using System.Threading.Tasks;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio base para operaciones de caja.
    /// </summary>
    public class CajaService
    {
        public virtual Task<decimal> ObtenerSaldoCajaAsync(DateTime fecha)
        {
            return Task.FromResult(0m);
        }

        public virtual Task RegistrarAjusteAsync(string tipo, decimal monto, string motivo)
        {
            return Task.CompletedTask;
        }

        public virtual Task<(decimal totalIngresos, decimal totalEgresos, decimal saldoFinal)> ObtenerResumenCajaPorFechaAsync(DateTime fecha)
        {
            return Task.FromResult((0m, 0m, 0m));
        }
    }
}
