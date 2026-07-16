using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SistemaVentas.Models;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio para gestionar operaciones de sorteos
    /// Incluye crear sorteos diarios, actualizar estados y registrar resultados
    /// </summary>
    public class SorteosService
    {
        private readonly BaseDatosService _baseDatos;
        private readonly AuditoriaService _auditoria;

        public SorteosService(BaseDatosService baseDatos, AuditoriaService auditoria)
        {
            _baseDatos = baseDatos ?? throw new ArgumentNullException(nameof(baseDatos));
            _auditoria = auditoria;
        }

        /// <summary>
        /// Obtiene todos los sorteos activos del sistema
        /// </summary>
        public async Task<List<Sorteo>> ObtenerSorteosActivosAsync()
        {
            string sql = @"
                SELECT Id, Nombre, Hora, MinutosCierreAntes
                FROM Sorteos
                WHERE Activo = 1
                ORDER BY Hora;
            ";

            var sorteos = new List<Sorteo>();

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
                                if (TimeSpan.TryParse(lector.GetString(2), out TimeSpan hora))
                                {
                                    sorteos.Add(new Sorteo
                                    {
                                        Id = lector.GetInt64(0),
                                        Nombre = lector.GetString(1),
                                        Hora = hora,
                                        MinutosCierreAntes = lector.GetInt32(3)
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo sorteos: {ex.Message}");
            }

            return sorteos;
        }

        /// <summary>
        /// Crea los sorteos del día basado en los sorteos activos
        /// Se ejecuta una sola vez al día al iniciar la aplicación
        /// </summary>
        public async Task CrearSorteosDelDiaAsync()
        {
            try
            {
                string hoy = DateTime.Today.ToString("yyyy-MM-dd");
                
                var sorteos = await ObtenerSorteosActivosAsync();

                foreach (var sorteo in sorteos)
                {
                    string sql = @"
                        INSERT OR IGNORE INTO SorteosDiarios 
                        (SorteoId, Fecha, Estado, FechaCreacion)
                        VALUES (@sorteoId, @fecha, 'Abierto', CURRENT_TIMESTAMP);
                    ";

                    var parametros = new Dictionary<string, object>
                    {
                        { "sorteoId", sorteo.Id },
                        { "fecha", hoy }
                    };

                    await _baseDatos.ExecuteNonQueryAsync(sql, parametros);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creando sorteos del día: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza los estados de los sorteos según la hora actual
        /// Estados: Abierto → Cerrado → Pendiente de resultado → Finalizado
        /// </summary>
        public async Task ActualizarEstadosSorteosAsync()
        {
            try
            {
                DateTime ahora = DateTime.Now;
                string hoy = DateTime.Today.ToString("yyyy-MM-dd");

                var sorteos = await ObtenerSorteosActivosAsync();

                foreach (var sorteo in sorteos)
                {
                    DateTime fechaHoraSorteo = DateTime.Today.Add(sorteo.Hora);
                    DateTime fechaHoraCierre = fechaHoraSorteo.AddMinutes(-sorteo.MinutosCierreAntes);

                    string nuevoEstado;

                    if (ahora < fechaHoraCierre)
                        nuevoEstado = "Abierto";
                    else if (ahora < fechaHoraSorteo)
                        nuevoEstado = "Cerrado";
                    else
                        nuevoEstado = "Pendiente de resultado";

                    string sql = @"
                        UPDATE SorteosDiarios
                        SET Estado = CASE
                            WHEN Resultado IS NOT NULL AND TRIM(Resultado) <> ''
                            THEN 'Finalizado'
                            ELSE @estado
                        END
                        WHERE SorteoId = @sorteoId AND Fecha = @fecha;
                    ";

                    var parametros = new Dictionary<string, object>
                    {
                        { "estado", nuevoEstado },
                        { "sorteoId", sorteo.Id },
                        { "fecha", hoy }
                    };

                    await _baseDatos.ExecuteNonQueryAsync(sql, parametros);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando estados: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra el resultado (número ganador) de un sorteo
        /// </summary>
        public async Task<(bool exito, string mensaje)> RegistrarResultadoAsync(
            long sorteoDiarioId, 
            string numeroGanador, 
            long usuarioId)
        {
            if (string.IsNullOrWhiteSpace(numeroGanador) || numeroGanador.Length != 2)
                return (false, "El número debe tener 2 dígitos");

            try
            {
                string sql = @"
                    UPDATE SorteosDiarios
                    SET Resultado = @numero, 
                        Estado = 'Finalizado',
                        FechaResultado = CURRENT_TIMESTAMP
                    WHERE Id = @id 
                    AND Estado IN ('Pendiente de resultado', 'Cerrado');
                ";

                var parametros = new Dictionary<string, object>
                {
                    { "numero", numeroGanador },
                    { "id", sorteoDiarioId }
                };

                int filas = await _baseDatos.ExecuteNonQueryAsync(sql, parametros);

                if (filas > 0)
                {
                    await _auditoria?.RegistrarAsync("RESULTADO", "Sorteos", 
                        $"Resultado {numeroGanador} registrado para sorteo {sorteoDiarioId}", usuarioId);
                    return (true, $"Resultado {numeroGanador} registrado exitosamente");
                }

                return (false, "No se pudo registrar el resultado");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registrando resultado: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene los sorteos del día con sus estados actualizados
        /// </summary>
        public async Task<List<SorteoDiario>> ObtenerSorteosDiarioAsync(DateTime fecha)
        {
            string sql = @"
                SELECT sd.Id, s.Nombre, s.Hora, s.MinutosCierreAntes, sd.Estado, COALESCE(sd.Resultado, '')
                FROM SorteosDiarios sd
                INNER JOIN Sorteos s ON s.Id = sd.SorteoId
                WHERE sd.Fecha = @fecha
                ORDER BY s.Hora;
            ";

            var sorteos = new List<SorteoDiario>();

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
                                if (TimeSpan.TryParse(lector.GetString(2), out TimeSpan hora))
                                {
                                    sorteos.Add(new SorteoDiario
                                    {
                                        Id = lector.GetInt64(0),
                                        Nombre = lector.GetString(1),
                                        Hora = hora,
                                        MinutosCierreAntes = lector.GetInt32(3),
                                        Estado = lector.GetString(4),
                                        Resultado = lector.GetString(5)
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo sorteos diarios: {ex.Message}");
            }

            return sorteos;
        }

        /// <summary>
        /// Obtiene el próximo sorteo a jugarse
        /// </summary>
        public async Task<SorteoDiario> ObtenerProximoSorteoAsync()
        {
            var sorteosDia = await ObtenerSorteosDiarioAsync(DateTime.Today);
            DateTime ahora = DateTime.Now;

            foreach (var sorteo in sorteosDia)
            {
                DateTime horaSorteo = DateTime.Today.Add(sorteo.Hora);
                if (horaSorteo > ahora && sorteo.Estado != "Finalizado")
                    return sorteo;
            }

            return null;
        }
    }
}
