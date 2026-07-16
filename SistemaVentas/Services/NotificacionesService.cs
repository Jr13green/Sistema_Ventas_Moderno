using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SistemaVentas.Models;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio para gestionar notificaciones del sistema
    /// Incluye crear, consultar y marcar notificaciones como leídas
    /// </summary>
    public class NotificacionesService
    {
        private readonly BaseDatosService _baseDatos;

        public NotificacionesService(BaseDatosService baseDatos)
        {
            _baseDatos = baseDatos ?? throw new ArgumentNullException(nameof(baseDatos));
        }

        /// <summary>
        /// Crea una nueva notificación
        /// </summary>
        public async Task<bool> CrearNotificacionAsync(string titulo, string detalle, string prioridad, string modulo)
        {
            try
            {
                string clave = $"{modulo}_{DateTime.Now:yyyyMMddHHmmss}";
                
                string sql = @"
                    INSERT INTO Notificaciones 
                    (Clave, Titulo, Detalle, Prioridad, Modulo, Activa, Leida, FechaActualizacion)
                    VALUES (@clave, @titulo, @detalle, @prioridad, @modulo, 1, 0, CURRENT_TIMESTAMP);
                ";

                var parametros = new Dictionary<string, object>
                {
                    { "clave", clave },
                    { "titulo", titulo },
                    { "detalle", detalle },
                    { "prioridad", prioridad }, // "Alta", "Media", "Baja"
                    { "modulo", modulo }
                };

                await _baseDatos.ExecuteNonQueryAsync(sql, parametros);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creando notificación: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene el conteo de notificaciones no leídas
        /// </summary>
        public async Task<int> ObtenerConteoNoLeidasAsync()
        {
            string sql = "SELECT COUNT(*) FROM Notificaciones WHERE Leida = 0 AND Activa = 1;";

            try
            {
                object resultado = await _baseDatos.ExecuteScalarAsync(sql);
                return Convert.ToInt32(resultado ?? 0);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error contando notificaciones: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Obtiene las notificaciones activas
        /// </summary>
        public async Task<List<Notificacion>> ObtenerNotificacionesActivasAsync(int limite = 10)
        {
            string sql = $@"
                SELECT Clave, Titulo, Detalle, Prioridad, Modulo, Leida, FechaActualizacion
                FROM Notificaciones
                WHERE Activa = 1
                ORDER BY FechaActualizacion DESC
                LIMIT {limite};
            ";

            var notificaciones = new List<Notificacion>();

            try
            {
                using (SqliteConnection conexion = _baseDatos.ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (SqliteCommand comando = new SqliteCommand(sql, conexion))
                    {
                        using (SqliteDataReader lector = await comando.ExecuteReaderAsync())
                        {
                            while (await lector.ReadAsync())
                            {
                                notificaciones.Add(new Notificacion
                                {
                                    Clave = lector.GetString(0),
                                    Titulo = lector.GetString(1),
                                    Detalle = lector.GetString(2),
                                    Prioridad = lector.GetString(3),
                                    Modulo = lector.GetString(4),
                                    Leida = lector.GetInt32(5) == 1,
                                    FechaActualizacion = DateTime.Parse(lector.GetString(6))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo notificaciones: {ex.Message}");
            }

            return notificaciones;
        }

        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        public async Task<bool> MarcarComoLeidaAsync(string clave)
        {
            string sql = @"
                UPDATE Notificaciones
                SET Leida = 1, FechaActualizacion = CURRENT_TIMESTAMP
                WHERE Clave = @clave;
            ";

            try
            {
                var parametros = new Dictionary<string, object> { { "clave", clave } };
                int filas = await _baseDatos.ExecuteNonQueryAsync(sql, parametros);
                return filas > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marcando como leída: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Desactiva una notificación
        /// </summary>
        public async Task<bool> DesactivarNotificacionAsync(string clave)
        {
            string sql = @"
                UPDATE Notificaciones
                SET Activa = 0, FechaActualizacion = CURRENT_TIMESTAMP
                WHERE Clave = @clave;
            ";

            try
            {
                var parametros = new Dictionary<string, object> { { "clave", clave } };
                int filas = await _baseDatos.ExecuteNonQueryAsync(sql, parametros);
                return filas > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error desactivando notificación: {ex.Message}");
                return false;
            }
        }
    }
}
