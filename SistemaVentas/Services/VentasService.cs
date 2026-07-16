using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio base para operaciones de ventas usadas por MVVM.
    /// </summary>
    public class VentasService
    {
        public virtual Task<decimal> ObtenerTotalVentasActivasPorFechaAsync(DateTime fecha)
        {
            return Task.FromResult(0m);
        }

        public virtual Task<(bool exito, string mensaje, long ventaId)> CrearVentaAsync(
            long usuarioId,
            List<(long sorteoDiarioId, string numero, decimal monto)> jugadas)
        {
            if (jugadas == null || !jugadas.Any())
                return Task.FromResult((false, "Debe ingresar al menos una jugada", 0L));

            long ventaId = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return Task.FromResult((true, "Venta creada correctamente", ventaId));
        }
    }
}
