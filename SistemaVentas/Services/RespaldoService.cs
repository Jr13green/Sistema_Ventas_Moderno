using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SistemaVentas.Models;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio para gestionar respaldos automáticos y manuales de la base de datos
    /// Incluye creación, restauración y limpieza de respaldos
    /// </summary>
    public class RespaldoService
    {
        private readonly BaseDatosService _baseDatos;
        private readonly ConfiguracionService _configuracion;
        private readonly AuditoriaService _auditoria;
        private readonly string _carpetaRespaldos;

        public RespaldoService(
            BaseDatosService baseDatos, 
            ConfiguracionService configuracion,
            AuditoriaService auditoria)
        {
            _baseDatos = baseDatos ?? throw new ArgumentNullException(nameof(baseDatos));
            _configuracion = configuracion;
            _auditoria = auditoria;
            _carpetaRespaldos = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Respaldos");
        }

        /// <summary>
        /// Crea un respaldo manual de la base de datos
        /// </summary>
        public async Task<(bool exito, string ruta, string mensaje)> CrearRespaldoManualAsync(
            string nombrePersonalizado = "")
        {
            try
            {
                Directory.CreateDirectory(_carpetaRespaldos);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string nombreArchivo = string.IsNullOrWhiteSpace(nombrePersonalizado)
                    ? $"Respaldo_Manual_{timestamp}.db"
                    : $"Respaldo_{nombrePersonalizado}_{timestamp}.db";

                string rutaDestino = Path.Combine(_carpetaRespaldos, nombreArchivo);

                using (SqliteConnection conexion = _baseDatos.ObtenerConexion())
                {
                    await conexion.OpenAsync();
                    using (SqliteCommand comando = new SqliteCommand(
                        $"VACUUM INTO '{rutaDestino.Replace("'", "''")}'", conexion))
                    {
                        await comando.ExecuteNonQueryAsync();
                    }
                }

                await _auditoria?.RegistrarAsync("RESPALDO", "Sistema", 
                    $"Respaldo manual creado: {nombreArchivo}");

                return (true, rutaDestino, $"Respaldo creado exitosamente: {nombreArchivo}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creando respaldo: {ex.Message}");
                return (false, "", $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Crea un respaldo automático (si corresponde según configuración)
        /// </summary>
        public async Task<bool> CrearRespaldoAutomaticoAsync()
        {
            try
            {
                if (!_configuracion.ObtenerBooleano("RespaldoAutomatico", true))
                    return false;

                string fechaHoy = DateTime.Today.ToString("yyyy-MM-dd");
                string ultimaFecha = _configuracion.ObtenerTexto("FechaUltimoRespaldoAutomatico", "");

                // Solo crear si no se creó hoy
                if (ultimaFecha == fechaHoy)
                    return false;

                var (exito, ruta, _) = await CrearRespaldoManualAsync("Automatico");

                if (exito)
                {
                    await _configuracion.GuardarAsync("FechaUltimoRespaldoAutomatico", fechaHoy);
                    await LimpiarRespaldosAntiguosAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en respaldo automático: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene la lista de respaldos disponibles
        /// </summary>
        public List<(string nombre, long tamanio, DateTime fecha)> ObtenerRespaldosDisponibles()
        {
            var respaldos = new List<(string, long, DateTime)>();

            try
            {
                if (!Directory.Exists(_carpetaRespaldos))
                    return respaldos;

                DirectoryInfo dir = new DirectoryInfo(_carpetaRespaldos);
                FileInfo[] archivos = dir.GetFiles("Respaldo*.db");

                foreach (FileInfo archivo in archivos)
                {
                    respaldos.Add((
                        archivo.Name,
                        archivo.Length,
                        archivo.CreationTime
                    ));
                }

                // Ordenar por fecha descendente
                respaldos.Sort((a, b) => b.fecha.CompareTo(a.fecha));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error listando respaldos: {ex.Message}");
            }

            return respaldos;
        }

        /// <summary>
        /// Limpia respaldos antiguos según la configuración
        /// </summary>
        private async Task LimpiarRespaldosAntiguosAsync()
        {
            try
            {
                int cantidad = _configuracion.ObtenerEntero("CantidadRespaldos", 15);
                cantidad = Math.Max(3, Math.Min(100, cantidad)); // Entre 3 y 100

                var respaldos = ObtenerRespaldosDisponibles();

                if (respaldos.Count > cantidad)
                {
                    for (int i = cantidad; i < respaldos.Count; i++)
                    {
                        try
                        {
                            string rutaArchivo = Path.Combine(_carpetaRespaldos, respaldos[i].nombre);
                            File.Delete(rutaArchivo);
                            System.Diagnostics.Debug.WriteLine($"Respaldo antiguo eliminado: {respaldos[i].nombre}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error eliminando respaldo: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error limpiando respaldos: {ex.Message}");
            }
        }

        /// <summary>
        /// Abre la carpeta de respaldos
        /// </summary>
        public void AbrirCarpetaRespaldos()
        {
            try
            {
                Directory.CreateDirectory(_carpetaRespaldos);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _carpetaRespaldos,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error abriendo carpeta: {ex.Message}");
            }
        }
    }
}
