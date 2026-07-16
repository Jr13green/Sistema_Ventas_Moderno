using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio para registrar eventos de auditoría en el sistema
    /// Mantiene un registro detallado de todas las operaciones sensibles
    /// </summary>
    public class AuditoriaService
    {
        private readonly BaseDatosService _baseDatos;

        public AuditoriaService(BaseDatosService baseDatos)
        {
            _baseDatos = baseDatos ?? throw new ArgumentNullException(nameof(baseDatos));
        }

        /// <summary>
        /// Registra un evento de auditoría
        /// </summary>
        public async Task RegistrarAsync(string accion, string modulo, string detalle, long? usuarioId = null)
        {
            try
            {
                string sql = @"
                    INSERT INTO Auditoria 
                    (Accion, Modulo, Detalle, UsuarioId, FechaCreacion)
                    VALUES (@accion, @modulo, @detalle, @usuarioId, CURRENT_TIMESTAMP);
                ";

                var parametros = new Dictionary<string, object>
                {
                    { "accion", accion },
                    { "modulo", modulo },
                    { "detalle", detalle },
                    { "usuarioId", usuarioId ?? (object)DBNull.Value }
                };

                await _baseDatos.ExecuteNonQueryAsync(sql, parametros);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registrando auditoría: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene todos los eventos de auditoría con paginación
        /// </summary>
        public async Task<List<(string accion, string modulo, string detalle, DateTime fecha)>> 
            ObtenerEventosAsync(int pagina = 1, int porPagina = 100)
        {
            string sql = $@"
                SELECT Accion, Modulo, Detalle, FechaCreacion
                FROM Auditoria
                ORDER BY Id DESC
                LIMIT {porPagina} OFFSET {(pagina - 1) * porPagina};
            ";

            var eventos = new List<(string, string, string, DateTime)>();

            try
            {
                using (var conexion = _baseDatos.ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (var comando = new Microsoft.Data.Sqlite.SqliteCommand(sql, conexion))
                    {
                        using (var lector = await comando.ExecuteReaderAsync())
                        {
                            while (await lector.ReadAsync())
                            {
                                eventos.Add((
                                    lector.GetString(0),
                                    lector.GetString(1),
                                    lector.GetString(2),
                                    DateTime.Parse(lector.GetString(3))
                                ));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo eventos: {ex.Message}");
            }

            return eventos;
        }

        /// <summary>
        /// Obtiene eventos de auditoría filtrados por módulo
        /// </summary>
        public async Task<List<(string accion, string detalle, DateTime fecha)>> 
            ObtenerEventosPorModuloAsync(string modulo, int limite = 50)
        {
            string sql = @"
                SELECT Accion, Detalle, FechaCreacion
                FROM Auditoria
                WHERE Modulo = @modulo
                ORDER BY Id DESC
                LIMIT @limite;
            ";

            var eventos = new List<(string, string, DateTime)>();

            try
            {
                using (var conexion = _baseDatos.ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (var comando = new Microsoft.Data.Sqlite.SqliteCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@modulo", modulo);
                        comando.Parameters.AddWithValue("@limite", limite);
                        
                        using (var lector = await comando.ExecuteReaderAsync())
                        {
                            while (await lector.ReadAsync())
                            {
                                eventos.Add((
                                    lector.GetString(0),
                                    lector.GetString(1),
                                    DateTime.Parse(lector.GetString(2))
                                ));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo eventos: {ex.Message}");
            }

            return eventos;
        }

        /// <summary>
        /// Obtiene el contador total de eventos de auditoría
        /// </summary>
        public async Task<int> ObtenerTotalEventosAsync()
        {
            string sql = "SELECT COUNT(*) FROM Auditoria;";

            try
            {
                object resultado = await _baseDatos.ExecuteScalarAsync(sql);
                return Convert.ToInt32(resultado ?? 0);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error contando eventos: {ex.Message}");
                return 0;
            }
        }
    }
}
