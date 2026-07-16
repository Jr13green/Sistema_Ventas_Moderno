using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio base para reportes.
    /// </summary>
    public class ReportesService
    {
        public virtual Task<(decimal ventas, decimal premios, decimal ganancia, int transacciones)> ObtenerResumenPeriodoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return Task.FromResult((0m, 0m, 0m, 0));
        }

        public virtual Task<List<(string nombre, decimal total, int ventas)>> ObtenerTopVendedoresAsync(DateTime fechaInicio, DateTime fechaFin, int limite)
        {
            return Task.FromResult(new List<(string, decimal, int)>());
        }

        public virtual Task<string> ExportarVentasCSVAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            const string encabezado = "Fecha,Venta,Premio,Ganancia";
            return Task.FromResult(encabezado);
        }
    }
}
