using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio para generar reportes e informes del sistema
    /// Analiza datos de ventas, premios, ganancias y rendimiento
    /// </summary>
    public class ReportesService
    {
        private readonly BaseDatosService _baseDatos;
        private readonly ConfiguracionService _configuracion;

        public ReportesService(BaseDatosService baseDatos, ConfiguracionService configuracion)
        {
            _baseDatos = baseDatos ?? throw new ArgumentNullException(nameof(baseDatos));
            _configuracion = configuracion;
        }

        /// <summary>
        /// Obtiene el reporte de ventas para un período específico
        /// </summary>
        public async Task<(decimal ventas, decimal premios, decimal ganancia, int transacciones)> 
            ObtenerResumenPeriodoAsync(DateTime inicio, DateTime fin)
        {
            try
            {
                string inicioTexto = inicio.ToString("yyyy-MM-dd");
                string finTexto = fin.ToString("yyyy-MM-dd");

                // Obtener ventas
                string ventasSql = @"
                    SELECT COALESCE(SUM(Total), 0)
                    FROM Ventas
                    WHERE Estado = 'Activa' 
                    AND DATE(FechaCreacion) BETWEEN @inicio AND @fin;
                ";

                var ventasParams = new Dictionary<string, object>
                {
                    { "inicio", inicioTexto },
                    { "fin", finTexto }
                };

                object ventasResult = await _baseDatos.ExecuteScalarAsync(ventasSql, ventasParams);
                decimal ventas = Convert.ToDecimal(ventasResult ?? 0);

                // Obtener premios
                string premiosSql = @"
                    SELECT COALESCE(SUM(dv.Monto * @multiplicador), 0)
                    FROM DetallesVenta dv
                    INNER JOIN Ventas v ON v.Id = dv.VentaId
                    INNER JOIN SorteosDiarios sd ON sd.Id = dv.SorteoDiarioId
                    WHERE v.Estado = 'Activa'
                    AND sd.Resultado IS NOT NULL
                    AND dv.Numero = sd.Resultado
                    AND DATE(sd.Fecha) BETWEEN @inicio AND @fin;
                ";

                decimal multiplicador = _configuracion.ObtenerMultiplicadorPremio();
                var premiosParams = new Dictionary<string, object>
                {
                    { "inicio", inicioTexto },
                    { "fin", finTexto },
                    { "multiplicador", multiplicador }
                };

                object premiosResult = await _baseDatos.ExecuteScalarAsync(premiosSql, premiosParams);
                decimal premios = Convert.ToDecimal(premiosResult ?? 0);

                // Obtener cantidad de transacciones
                string transaccionesSql = @"
                    SELECT COUNT(*)
                    FROM Ventas
                    WHERE Estado = 'Activa'
                    AND DATE(FechaCreacion) BETWEEN @inicio AND @fin;
                ";

                object transaccionesResult = await _baseDatos.ExecuteScalarAsync(transaccionesSql, ventasParams);
                int transacciones = Convert.ToInt32(transaccionesResult ?? 0);

                decimal ganancia = ventas - premios;

                return (ventas, premios, ganancia, transacciones);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generando reporte: {ex.Message}");
                return (0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Obtiene el top de vendedores para un período
        /// </summary>
        public async Task<List<(string nombre, decimal total, int ventas)>> 
            ObtenerTopVendedoresAsync(DateTime inicio, DateTime fin, int cantidad = 5)
        {
            string sql = $@"
                SELECT u.NombreCompleto, COALESCE(SUM(v.Total), 0), COUNT(v.Id)
                FROM Usuarios u
                LEFT JOIN Ventas v ON v.UsuarioId = u.Id 
                    AND v.Estado = 'Activa'
                    AND DATE(v.FechaCreacion) BETWEEN @inicio AND @fin
                WHERE u.Rol = 'Vendedor'
                GROUP BY u.Id, u.NombreCompleto
                ORDER BY SUM(v.Total) DESC
                LIMIT {cantidad};
            ";

            var vendedores = new List<(string, decimal, int)>();

            try
            {
                using (var conexion = _baseDatos.ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (var comando = new Microsoft.Data.Sqlite.SqliteCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@inicio", inicio.ToString("yyyy-MM-dd"));
                        comando.Parameters.AddWithValue("@fin", fin.ToString("yyyy-MM-dd"));

                        using (var lector = await comando.ExecuteReaderAsync())
                        {
                            while (await lector.ReadAsync())
                            {
                                vendedores.Add((
                                    lector.GetString(0),
                                    Convert.ToDecimal(lector.GetDouble(1)),
                                    Convert.ToInt32(lector.GetDouble(2))
                                ));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo top vendedores: {ex.Message}");
            }

            return vendedores;
        }

        /// <summary>
        /// Obtiene información de ganadores del período
        /// </summary>
        public async Task<List<(string codigoVenta, string vendedor, string numero, decimal monto)>> 
            ObtenerGanadoresAsync(DateTime inicio, DateTime fin)
        {
            string sql = @"
                SELECT v.Codigo, u.NombreCompleto, dv.Numero, (dv.Monto * @multiplicador)
                FROM DetallesVenta dv
                INNER JOIN Ventas v ON v.Id = dv.VentaId
                INNER JOIN Usuarios u ON u.Id = v.UsuarioId
                INNER JOIN SorteosDiarios sd ON sd.Id = dv.SorteoDiarioId
                WHERE v.Estado = 'Activa'
                AND sd.Resultado IS NOT NULL
                AND dv.Numero = sd.Resultado
                AND DATE(sd.Fecha) BETWEEN @inicio AND @fin
                ORDER BY sd.Fecha DESC;
            ";

            var ganadores = new List<(string, string, string, decimal)>();

            try
            {
                decimal multiplicador = _configuracion.ObtenerMultiplicadorPremio();

                using (var conexion = _baseDatos.ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (var comando = new Microsoft.Data.Sqlite.SqliteCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@inicio", inicio.ToString("yyyy-MM-dd"));
                        comando.Parameters.AddWithValue("@fin", fin.ToString("yyyy-MM-dd"));
                        comando.Parameters.AddWithValue("@multiplicador", multiplicador);

                        using (var lector = await comando.ExecuteReaderAsync())
                        {
                            while (await lector.ReadAsync())
                            {
                                ganadores.Add((
                                    lector.GetString(0),
                                    lector.GetString(1),
                                    lector.GetString(2),
                                    Convert.ToDecimal(lector.GetDouble(3))
                                ));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo ganadores: {ex.Message}");
            }

            return ganadores;
        }

        /// <summary>
        /// Exporta los datos de ventas a formato CSV
        /// </summary>
        public async Task<string> ExportarVentasCSVAsync(DateTime inicio, DateTime fin)
        {
            string sql = @"
                SELECT 
                    v.Codigo, v.FechaCreacion, u.NombreCompleto, v.Estado,
                    s.Nombre, dv.Numero, dv.Monto
                FROM Ventas v
                INNER JOIN Usuarios u ON u.Id = v.UsuarioId
                INNER JOIN DetallesVenta dv ON dv.VentaId = v.Id
                INNER JOIN SorteosDiarios sd ON sd.Id = dv.SorteoDiarioId
                INNER JOIN Sorteos s ON s.Id = sd.SorteoId
                WHERE DATE(v.FechaCreacion) BETWEEN @inicio AND @fin
                ORDER BY v.FechaCreacion DESC;
            ";

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Codigo;Fecha;Vendedor;Estado;Sorteo;Numero;Monto");

            try
            {
                using (var conexion = _baseDatos.ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (var comando = new Microsoft.Data.Sqlite.SqliteCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@inicio", inicio.ToString("yyyy-MM-dd"));
                        comando.Parameters.AddWithValue("@fin", fin.ToString("yyyy-MM-dd"));

                        using (var lector = await comando.ExecuteReaderAsync())
                        {
                            while (await lector.ReadAsync())
                            {
                                csv.AppendLine($"{lector.GetString(0)};{lector.GetString(1)};{lector.GetString(2)};{lector.GetString(3)};{lector.GetString(4)};{lector.GetString(5)};{lector.GetDouble(6):N2}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exportando CSV: {ex.Message}");
            }

            return csv.ToString();
        }
    }
}
