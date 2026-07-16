using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SistemaVentas.Models;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio para gestionar todas las operaciones de ventas
    /// Incluye crear, consultar, anular y generar reportes de ventas
    /// </summary>
    public class VentasService
    {
        private readonly BaseDatosService _baseDatos;
        private readonly AuditoriaService _auditoria;

        public VentasService(BaseDatosService baseDatos, AuditoriaService auditoria)
        {
            _baseDatos = baseDatos ?? throw new ArgumentNullException(nameof(baseDatos));
            _auditoria = auditoria;
        }

        /// <summary>
        /// Crea una nueva venta con sus detalles
        /// </summary>
        public async Task<(bool exito, string mensaje, long ventaId)> CrearVentaAsync(
            long usuarioId, 
            List<(long sorteoDiarioId, string numero, decimal monto)> jugadas)
        {
            if (jugadas == null || jugadas.Count == 0)
                return (false, "La venta debe tener al menos una jugada", 0);

            try
            {
                using (SqliteConnection conexion = _baseDatos.ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (SqliteTransaction transaccion = conexion.BeginTransaction())
                    {
                        // Calcular total
                        decimal total = 0;
                        foreach (var (_, _, monto) in jugadas)
                            total += monto;

                        // Crear venta
                        string ventaSql = @"
                            INSERT INTO Ventas 
                            (Codigo, UsuarioId, Total, Estado, FechaCreacion)
                            VALUES (@codigo, @usuarioId, @total, 'Activa', CURRENT_TIMESTAMP);
                            SELECT last_insert_rowid();
                        ";

                        string codigo = $"V-{DateTime.Now:yyyyMMdd-HHmmssfff}";
                        
                        long ventaId = 0;
                        using (SqliteCommand comando = new SqliteCommand(ventaSql, conexion, transaccion))
                        {
                            comando.Parameters.AddWithValue("@codigo", codigo);
                            comando.Parameters.AddWithValue("@usuarioId", usuarioId);
                            comando.Parameters.AddWithValue("@total", total);
                            ventaId = Convert.ToInt64(await comando.ExecuteScalarAsync());
                        }

                        // Insertar detalles
                        foreach (var (sorteoDiarioId, numero, monto) in jugadas)
                        {
                            string detalleSql = @"
                                INSERT INTO DetallesVenta 
                                (VentaId, SorteoDiarioId, Numero, Monto, FechaCreacion)
                                VALUES (@ventaId, @sorteoDiarioId, @numero, @monto, CURRENT_TIMESTAMP);
                            ";

                            using (SqliteCommand comando = new SqliteCommand(detalleSql, conexion, transaccion))
                            {
                                comando.Parameters.AddWithValue("@ventaId", ventaId);
                                comando.Parameters.AddWithValue("@sorteoDiarioId", sorteoDiarioId);
                                comando.Parameters.AddWithValue("@numero", numero.PadLeft(2, '0'));
                                comando.Parameters.AddWithValue("@monto", monto);
                                await comando.ExecuteNonQueryAsync();
                            }
                        }

                        transaccion.Commit();

                        // Registrar en auditoría
                        await _auditoria?.RegistrarAsync("CREAR", "Ventas", 
                            $"Venta {codigo} creada por L {total:N2}", usuarioId);

                        return (true, $"Venta {codigo} registrada exitosamente", ventaId);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creando venta: {ex.Message}");
                return (false, $"Error: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Anula una venta (cambia estado a 'Anulada')
        /// </summary>
        public async Task<(bool exito, string mensaje)> AnularVentaAsync(long ventaId, long usuarioId)
        {
            string sql = @"
                UPDATE Ventas
                SET Estado = 'Anulada'
                WHERE Id = @id AND Estado = 'Activa';
            ";

            try
            {
                var parametros = new Dictionary<string, object> { { "id", ventaId } };
                int filas = await _baseDatos.ExecuteNonQueryAsync(sql, parametros);

                if (filas > 0)
                {
                    await _auditoria?.RegistrarAsync("ANULAR", "Ventas", 
                        $"Venta {ventaId} anulada", usuarioId);
                    return (true, "Venta anulada correctamente");
                }

                return (false, "No se pudo anular la venta o ya estaba anulada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error anulando venta: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene el total de ventas activas para una fecha
        /// </summary>
        public async Task<decimal> ObtenerTotalVentasActivasPorFechaAsync(DateTime fecha)
        {
            string sql = @"
                SELECT COALESCE(SUM(Total), 0)
                FROM Ventas
                WHERE Estado = 'Activa' 
                AND DATE(FechaCreacion) = @fecha;
            ";

            try
            {
                var parametros = new Dictionary<string, object> 
                { 
                    { "fecha", fecha.ToString("yyyy-MM-dd") } 
                };
                object resultado = await _baseDatos.ExecuteScalarAsync(sql, parametros);
                return Convert.ToDecimal(resultado ?? 0);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculando total: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Obtiene el detalle completo de una venta
        /// </summary>
        public async Task<Venta> ObtenerVentaAsync(long ventaId)
        {
            string sql = @"
                SELECT v.Id, v.Codigo, v.UsuarioId, v.Total, v.Estado, v.FechaCreacion
                FROM Ventas v
                WHERE v.Id = @id
                LIMIT 1;
            ";

            try
            {
                using (SqliteConnection conexion = _baseDatos.ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (SqliteCommand comando = new SqliteCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@id", ventaId);
                        using (SqliteDataReader lector = await comando.ExecuteReaderAsync())
                        {
                            if (await lector.ReadAsync())
                            {
                                return new Venta
                                {
                                    Id = lector.GetInt64(0),
                                    Codigo = lector.GetString(1),
                                    UsuarioId = lector.GetInt64(2),
                                    Total = Convert.ToDecimal(lector.GetDouble(3)),
                                    Estado = lector.GetString(4),
                                    FechaCreacion = DateTime.Parse(lector.GetString(5))
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo venta: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Obtiene todas las ventas de un vendedor en una fecha
        /// </summary>
        public async Task<List<Venta>> ObtenerVentasVendedorPorFechaAsync(long usuarioId, DateTime fecha)
        {
            string sql = @"
                SELECT Id, Codigo, UsuarioId, Total, Estado, FechaCreacion
                FROM Ventas
                WHERE UsuarioId = @usuarioId 
                AND DATE(FechaCreacion) = @fecha
                ORDER BY FechaCreacion DESC;
            ";

            var ventas = new List<Venta>();

            try
            {
                using (SqliteConnection conexion = _baseDatos.ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (SqliteCommand comando = new SqliteCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@usuarioId", usuarioId);
                        comando.Parameters.AddWithValue("@fecha", fecha.ToString("yyyy-MM-dd"));
                        using (SqliteDataReader lector = await comando.ExecuteReaderAsync())
                        {
                            while (await lector.ReadAsync())
                            {
                                ventas.Add(new Venta
                                {
                                    Id = lector.GetInt64(0),
                                    Codigo = lector.GetString(1),
                                    UsuarioId = lector.GetInt64(2),
                                    Total = Convert.ToDecimal(lector.GetDouble(3)),
                                    Estado = lector.GetString(4),
                                    FechaCreacion = DateTime.Parse(lector.GetString(5))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo ventas: {ex.Message}");
            }

            return ventas;
        }

        /// <summary>
        /// Obtiene la cantidad de jugadas de una venta
        /// </summary>
        public async Task<int> ObtenerCantidadJugadasAsync(long ventaId)
        {
            string sql = "SELECT COUNT(*) FROM DetallesVenta WHERE VentaId = @ventaId;";

            try
            {
                var parametros = new Dictionary<string, object> { { "ventaId", ventaId } };
                object resultado = await _baseDatos.ExecuteScalarAsync(sql, parametros);
                return Convert.ToInt32(resultado ?? 0);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo jugadas: {ex.Message}");
                return 0;
            }
        }
    }
}
