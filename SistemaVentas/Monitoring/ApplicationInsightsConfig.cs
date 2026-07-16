using System;
using System.Diagnostics;

namespace SistemaVentas.Monitoring
{
    /// <summary>
    /// Configuración de Application Insights para telemetría en producción.
    /// Proporciona un cliente ligero de telemetría cuando Application Insights
    /// no está disponible (evita dependencia obligatoria del SDK).
    /// </summary>
    public class ApplicationInsightsConfig
    {
        private readonly string _connectionString;
        private readonly bool   _habilitado;

        public ApplicationInsightsConfig(string? connectionString = null)
        {
            _connectionString = connectionString ?? string.Empty;
            _habilitado       = !string.IsNullOrWhiteSpace(_connectionString);
        }

        public bool Habilitado => _habilitado;

        /// <summary>
        /// Registra un evento personalizado de telemetría.
        /// En producción, usar el SDK oficial de Application Insights.
        /// </summary>
        public void TrackEvent(string eventName,
            System.Collections.Generic.Dictionary<string, string>? properties = null)
        {
            if (!_habilitado)
            {
                Debug.WriteLine($"[Telemetry] Event: {eventName} | Props: {FormatProps(properties)}");
                return;
            }

            // Aquí se integraría el TelemetryClient de Microsoft.ApplicationInsights
            // _telemetryClient.TrackEvent(eventName, properties);
        }

        /// <summary>Registra una excepción para diagnóstico.</summary>
        public void TrackException(Exception ex,
            System.Collections.Generic.Dictionary<string, string>? properties = null)
        {
            if (!_habilitado)
            {
                Debug.WriteLine($"[Telemetry] Exception: {ex.GetType().Name} - {ex.Message}");
                return;
            }

            // _telemetryClient.TrackException(ex, properties);
        }

        /// <summary>Registra una métrica de rendimiento.</summary>
        public void TrackMetric(string name, double value)
        {
            if (!_habilitado)
            {
                Debug.WriteLine($"[Telemetry] Metric: {name} = {value}");
                return;
            }

            // _telemetryClient.TrackMetric(name, value);
        }

        /// <summary>Registra una dependencia (DB, API externa, etc.).</summary>
        public void TrackDependency(string type, string name, string data,
            DateTime startTime, TimeSpan duration, bool success)
        {
            if (!_habilitado)
            {
                Debug.WriteLine(
                    $"[Telemetry] Dependency: {type}/{name} ({(success ? "OK" : "FAIL")}) {duration.TotalMs():N0}ms");
                return;
            }

            // _telemetryClient.TrackDependency(type, name, data, startTime, duration, success);
        }

        private static string FormatProps(
            System.Collections.Generic.Dictionary<string, string>? props)
        {
            if (props == null || props.Count == 0) return "{}";
            return "{" + string.Join(", ", props) + "}";
        }
    }

    internal static class TimeSpanExtensions
    {
        internal static double TotalMs(this TimeSpan ts) => ts.TotalMilliseconds;
    }
}
