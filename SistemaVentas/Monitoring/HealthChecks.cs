using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaVentas.Services;

namespace SistemaVentas.Monitoring
{
    /// <summary>
    /// Estado de salud de un componente del sistema.
    /// </summary>
    public class HealthStatus
    {
        public string  Nombre    { get; set; } = string.Empty;
        public bool    Saludable { get; set; }
        public string  Mensaje   { get; set; } = string.Empty;
        public TimeSpan Duracion { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Resultado global de salud del sistema.
    /// </summary>
    public class HealthReport
    {
        public bool   Saludable  { get; set; }
        public string Estado     { get; set; } = string.Empty;
        public List<HealthStatus> Componentes { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public TimeSpan DuracionTotal { get; set; }
    }

    /// <summary>
    /// Servicio de health checks para monitorear la salud del sistema.
    /// Verifica base de datos, espacio en disco y memoria disponible.
    /// </summary>
    public class HealthCheckService
    {
        private readonly BaseDatosService _baseDatos;

        public HealthCheckService(BaseDatosService baseDatos)
        {
            _baseDatos = baseDatos;
        }

        /// <summary>
        /// Ejecuta todos los health checks y retorna el reporte consolidado.
        /// </summary>
        public async Task<HealthReport> CheckAsync()
        {
            var inicio      = DateTime.UtcNow;
            var componentes = new List<HealthStatus>();

            var dbCheck     = await CheckDatabaseAsync();
            var memCheck    = CheckMemory();
            var diskCheck   = CheckDisk();

            componentes.Add(dbCheck);
            componentes.Add(memCheck);
            componentes.Add(diskCheck);

            var todosSaludables = componentes.TrueForAll(c => c.Saludable);

            return new HealthReport
            {
                Saludable    = todosSaludables,
                Estado       = todosSaludables ? "Healthy" : "Unhealthy",
                Componentes  = componentes,
                Timestamp    = inicio,
                DuracionTotal = DateTime.UtcNow - inicio
            };
        }

        // ── Checks individuales ───────────────────────────────────────────────

        private async Task<HealthStatus> CheckDatabaseAsync()
        {
            var inicio = DateTime.UtcNow;
            try
            {
                var integra = await _baseDatos.VerificarIntegridadAsync();
                return new HealthStatus
                {
                    Nombre    = "BaseDatos",
                    Saludable = integra,
                    Mensaje   = integra ? "Base de datos operativa." : "Fallo de integridad detectado.",
                    Duracion  = DateTime.UtcNow - inicio
                };
            }
            catch (Exception ex)
            {
                return new HealthStatus
                {
                    Nombre    = "BaseDatos",
                    Saludable = false,
                    Mensaje   = $"Error al verificar base de datos: {ex.Message}",
                    Duracion  = DateTime.UtcNow - inicio
                };
            }
        }

        private static HealthStatus CheckMemory()
        {
            var inicio = DateTime.UtcNow;
            try
            {
                var usedMb = GC.GetTotalMemory(false) / 1_048_576.0;
                var umbral = 500.0; // 500 MB
                var saludable = usedMb < umbral;
                return new HealthStatus
                {
                    Nombre    = "Memoria",
                    Saludable = saludable,
                    Mensaje   = $"Memoria usada: {usedMb:N1} MB (umbral: {umbral} MB).",
                    Duracion  = DateTime.UtcNow - inicio
                };
            }
            catch (Exception ex)
            {
                return new HealthStatus
                {
                    Nombre    = "Memoria",
                    Saludable = false,
                    Mensaje   = $"Error al verificar memoria: {ex.Message}",
                    Duracion  = DateTime.UtcNow - inicio
                };
            }
        }

        private static HealthStatus CheckDisk()
        {
            var inicio = DateTime.UtcNow;
            try
            {
                var drive     = new System.IO.DriveInfo(System.IO.Directory.GetCurrentDirectory());
                var libreGb   = drive.AvailableFreeSpace / 1_073_741_824.0;
                var umbralGb  = 1.0; // 1 GB mínimo
                var saludable = libreGb >= umbralGb;
                return new HealthStatus
                {
                    Nombre    = "Disco",
                    Saludable = saludable,
                    Mensaje   = $"Espacio libre: {libreGb:N2} GB (umbral: {umbralGb} GB).",
                    Duracion  = DateTime.UtcNow - inicio
                };
            }
            catch (Exception ex)
            {
                return new HealthStatus
                {
                    Nombre    = "Disco",
                    Saludable = false,
                    Mensaje   = $"Error al verificar disco: {ex.Message}",
                    Duracion  = DateTime.UtcNow - inicio
                };
            }
        }
    }
}
