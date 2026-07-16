using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SistemaVentas.Models;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio para gestionar todas las operaciones de caja
    /// Incluye ajustes, movimientos y cierre de caja
    /// </summary>
    public class CajaService
    {
        private readonly BaseDatosService _baseDatos;
        private readonly AuditoriaService _auditoria;

        public CajaService(BaseDatosService baseDatos, AuditoriaService auditoria)
        {
            _baseDatos = baseDatos ?? throw new ArgumentNullException(nameof(baseDatos));
            _auditoria = auditoria;
        }

        /// <summary>
        /// Registra un ajuste de caja (entrada o salida)
        /// </summary>
        public async Task<(bool exito, string mensaje)> RegistrarAjusteAsync(
            string tipo, decimal monto, string motivo, long usuarioId)
        {
            if (monto <= 0)
                return (false, "El monto debe ser mayor a cero");

            if (string.IsNullOrWhiteSpace(tipo) || (tipo != "Entrada" && tipo != "Salida"))
                return (false, "Tipo de ajuste inválido");

            try
            {
                string sql = @"
                    INSERT INTO AjustesCaja 
                    (Tipo, Monto, Motivo, UsuarioId, FechaCreacion)
                    VALUES (@tipo, @monto, @motivo, @usuarioId, CURRENT_TIMESTAMP);
                ";

                var parametros = new Dictionary<string, object>
                {
                    { "tipo", tipo },
                    { "monto", monto },
                    { "motivo", motivo ?? "" },
                    { "usuarioId", usuarioId }
                };

                await _baseDatos.ExecuteNonQueryAsync(sql, parametros);
                await _auditoria?.RegistrarAsync("AJUSTE", "Caja", 
                    $"{tipo} de L {monto:N2}: {motivo}", usuarioId);

                return (true, $"Ajuste de {tipo} registrado correctamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registrando ajuste: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene el total de movimientos de caja para una fecha
        /// </summary>
        public async Task<(decimal entradas, decimal salidas)> ObtenerResumenCajaPorFechaAsync(DateTime fecha)
        {
            try
            {
                string sql = @"
                    SELECT 
                        COALESCE(SUM(CASE WHEN Tipo = 'Entrada' THEN Monto ELSE 0 END), 0) AS Entradas,
                        COALESCE(SUM(CASE WHEN Tipo = 'Salida' THEN Monto ELSE 0 END), 0) AS Salidas
                    FROM AjustesCaja
                    WHERE DATE(FechaCreacion) = @fecha;
                ";

                var parametros = new Dictionary<string, object>
                {
                    { "fecha", fecha.ToString("yyyy-MM-dd") }
                };

                using (SqliteConnection conexion = _baseDatos.ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (SqliteCommand comando = new SqliteCommand(sql, conexion))
                    {
                        foreach (var param in parametros)
                            comando.Parameters.AddWithValue("@" + param.Key, param.Value);

                        using (SqliteDataReader lector = await comando.ExecuteReaderAsync())
                        {
                            if (await lector.ReadAsync())
                            {
                                decimal entradas = Convert.ToDecimal(lector.GetDouble(0));
                                decimal salidas = Convert.ToDecimal(lector.GetDouble(1));
                                return (entradas, salidas);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo resumen: {ex.Message}");
            }

            return (0, 0);
        }

        /// <summary>
        /// Calcula el saldo actual de caja (ventas - premios + ajustes)
        /// </summary>
        public async Task<decimal> ObtenerSaldoCajaAsync(DateTime fecha, decimal multiplicadorPremio = 70)
        {
            try
            {
                // Obtener ventas del día
                string ventasSql = @"
                    SELECT COALESCE(SUM(Total), 0) FROM Ventas 
                    WHERE Estado = 'Activa' AND DATE(FechaCreacion) = @fecha;
                ";

                var ventasParams = new Dictionary<string, object>
                {
                    { "fecha", fecha.ToString("yyyy-MM-dd") }
                };

                object ventasResult = await _baseDatos.ExecuteScalarAsync(ventasSql, ventasParams);
                decimal ventas = Convert.ToDecimal(ventasResult ?? 0);

                // Obtener premios pagados
                string premiosSql = @"
                    SELECT COALESCE(SUM(dv.Monto * @multiplicador), 0)
                    FROM DetallesVenta dv
                    INNER JOIN Ventas v ON v.Id = dv.VentaId
                    INNER JOIN SorteosDiarios sd ON sd.Id = dv.SorteoDiarioId
                    WHERE v.Estado = 'Activa'
                    AND sd.Resultado IS NOT NULL
                    AND dv.Numero = sd.Resultado
                    AND DATE(sd.Fecha) = @fecha;
                ";

                var premiosParams = new Dictionary<string, object>
                {
                    { "fecha", fecha.ToString("yyyy-MM-dd") },
                    { "multiplicador", multiplicadorPremio }
                };

                object premiosResult = await _baseDatos.ExecuteScalarAsync(premiosSql, premiosParams);
                decimal premios = Convert.ToDecimal(premiosResult ?? 0);

                // Obtener ajustes de caja
                var (entradas, salidas) = await ObtenerResumenCajaPorFechaAsync(fecha);

                return ventas - premios + entradas - salidas;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculando saldo: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Obtiene el historial de ajustes de caja para una fecha
        /// </summary>
        public async Task<List<AjusteCaja>> ObtenerHistorialAjustesAsync(DateTime fecha)
        {
            string sql = @"
                SELECT Id, Tipo, Monto, Motivo, FechaCreacion
                FROM AjustesCaja
                WHERE DATE(FechaCreacion) = @fecha
                ORDER BY FechaCreacion DESC;
            ";

            var ajustes = new List<AjusteCaja>();

            try
            {
                using (SqliteConnection conexion = _baseDatos.ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (SqliteCommand comando = new SqliteCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@fecha", fecha.ToString("yyyy-MM-dd"));
                        using (SqliteDataReader lector = await comando.ExecuteReaderAsync())
                        {
                            while (await lector.ReadAsync())
                            {
                                ajustes.Add(new AjusteCaja
                                {
                                    Id = lector.GetInt64(0),
                                    Tipo = lector.GetString(1),
                                    Monto = Convert.ToDecimal(lector.GetDouble(2)),
                                    Motivo = lector.GetString(3),
                                    FechaCreacion = DateTime.Parse(lector.GetString(4))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo ajustes: {ex.Message}");
            }

            return ajustes;
        }
    }
}
