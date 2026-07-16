using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace SistemaVentas.Monitoring
{
    /// <summary>
    /// OpenTelemetry configuration for distributed tracing and metrics.
    /// Provides ActivitySource and Meter for instrumentation.
    /// </summary>
    public static class OpenTelemetryConfig
    {
        public const string ServiceName    = "SistemaVentas";
        public const string ServiceVersion = "1.0.0";

        // ActivitySource for distributed tracing
        public static readonly ActivitySource ActivitySource =
            new(ServiceName, ServiceVersion);

        // Meter for custom metrics
        public static readonly Meter Meter = new(ServiceName, ServiceVersion);

        // Custom counters
        public static readonly Counter<long> VentasCounter =
            Meter.CreateCounter<long>("ventas.total", "count", "Total de ventas procesadas");

        public static readonly Counter<long> ErroresCounter =
            Meter.CreateCounter<long>("errores.total", "count", "Total de errores del sistema");

        public static readonly Histogram<double> VentaDurationMs =
            Meter.CreateHistogram<double>("venta.duracion_ms", "ms", "Duración de operaciones de venta");

        public static readonly ObservableGauge<long> UsuariosActivosGauge =
            Meter.CreateObservableGauge<long>("usuarios.activos", () => _usuariosActivos,
                "count", "Usuarios activos en el sistema");

        private static long _usuariosActivos;

        public static void SetUsuariosActivos(long count) => _usuariosActivos = count;
    }

    /// <summary>Tracing helper methods.</summary>
    public static class Tracing
    {
        public static Activity? StartActivity(string name,
            ActivityKind kind = ActivityKind.Internal)
            => OpenTelemetryConfig.ActivitySource.StartActivity(name, kind);

        public static void RecordVenta(decimal monto, string usuario)
        {
            OpenTelemetryConfig.VentasCounter.Add(1,
                new KeyValuePair<string, object?>("usuario", usuario),
                new KeyValuePair<string, object?>("monto_rango", GetMontoRango(monto)));
        }

        public static void RecordError(string tipo, string operacion)
        {
            OpenTelemetryConfig.ErroresCounter.Add(1,
                new KeyValuePair<string, object?>("tipo", tipo),
                new KeyValuePair<string, object?>("operacion", operacion));
        }

        private static string GetMontoRango(decimal monto) => monto switch
        {
            < 100m   => "pequeño",
            < 1000m  => "mediano",
            < 10000m => "grande",
            _        => "muy_grande"
        };
    }
}
