using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SistemaVentas.Monitoring
{
    /// <summary>
    /// Registro de una métrica con timestamp, valor y etiquetas.
    /// </summary>
    public sealed class MetricPoint
    {
        public string   Name      { get; }
        public double   Value     { get; }
        public DateTime Timestamp { get; }
        public IReadOnlyDictionary<string, string> Tags { get; }

        public MetricPoint(string name, double value,
            Dictionary<string, string>? tags = null)
        {
            Name      = name;
            Value     = value;
            Timestamp = DateTime.UtcNow;
            Tags      = tags ?? new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Servicio de métricas ligero con soporte para:
    /// - Contadores (incrementales)
    /// - Gauges (valor instantáneo)
    /// - Histogramas (distribución de valores con percentiles)
    /// - Medición de duración (stopwatch helper)
    /// </summary>
    public class MetricsService
    {
        private readonly ConcurrentDictionary<string, long>              _counters    = new();
        private readonly ConcurrentDictionary<string, double>            _gauges      = new();
        private readonly ConcurrentDictionary<string, List<double>>      _histograms  = new();
        private readonly ConcurrentQueue<MetricPoint>                    _recentPoints = new();
        private const int MaxRecentPoints = 1000;

        // ── Contadores ────────────────────────────────────────────────────────

        /// <summary>Incrementa un contador por el valor indicado (default 1).</summary>
        public void Increment(string name, long amount = 1,
            Dictionary<string, string>? tags = null)
        {
            _counters.AddOrUpdate(name, amount, (_, v) => v + amount);
            Enqueue(new MetricPoint(name, amount, tags));
        }

        /// <summary>Obtiene el valor actual de un contador.</summary>
        public long GetCounter(string name)
            => _counters.TryGetValue(name, out var v) ? v : 0L;

        // ── Gauges ────────────────────────────────────────────────────────────

        /// <summary>Establece el valor actual de un gauge.</summary>
        public void SetGauge(string name, double value,
            Dictionary<string, string>? tags = null)
        {
            _gauges[name] = value;
            Enqueue(new MetricPoint(name, value, tags));
        }

        /// <summary>Obtiene el valor actual de un gauge.</summary>
        public double GetGauge(string name)
            => _gauges.TryGetValue(name, out var v) ? v : 0d;

        // ── Histogramas ───────────────────────────────────────────────────────

        /// <summary>Registra un valor en un histograma.</summary>
        public void RecordHistogram(string name, double value,
            Dictionary<string, string>? tags = null)
        {
            _histograms.GetOrAdd(name, _ => new List<double>())
                       .Add(value);          // thread-safe for reads, lock for writes is implicit
            Enqueue(new MetricPoint(name, value, tags));
        }

        /// <summary>Calcula el percentil P del histograma.</summary>
        public double GetPercentile(string name, double percentile)
        {
            if (!_histograms.TryGetValue(name, out var values) || values.Count == 0)
                return 0;

            var sorted = values.OrderBy(v => v).ToList();
            var index  = (int)Math.Ceiling(percentile / 100.0 * sorted.Count) - 1;
            return sorted[Math.Max(0, index)];
        }

        // ── Duración ──────────────────────────────────────────────────────────

        /// <summary>
        /// Mide la duración de una operación y la registra en el histograma
        /// correspondiente. Devuelve el Stopwatch iniciado para que el llamador
        /// pueda detenerlo cuando sea necesario.
        /// </summary>
        public IDisposable MeasureDuration(string metricName,
            Dictionary<string, string>? tags = null)
        {
            return new DurationMeasurement(this, metricName, tags);
        }

        // ── Resumen ───────────────────────────────────────────────────────────

        /// <summary>Obtiene un resumen de todas las métricas actuales.</summary>
        public MetricsSummary GetSummary()
        {
            return new MetricsSummary
            {
                Counters   = new Dictionary<string, long>(_counters),
                Gauges     = new Dictionary<string, double>(_gauges),
                Histograms = _histograms.ToDictionary(
                    kvp => kvp.Key,
                    kvp =>
                    {
                        var sorted = kvp.Value.OrderBy(v => v).ToList();
                        return new HistogramSummary
                        {
                            Count  = sorted.Count,
                            Min    = sorted.FirstOrDefault(),
                            Max    = sorted.LastOrDefault(),
                            Mean   = sorted.Count > 0 ? sorted.Average() : 0,
                            P50    = GetPercentile(kvp.Key, 50),
                            P95    = GetPercentile(kvp.Key, 95),
                            P99    = GetPercentile(kvp.Key, 99)
                        };
                    })
            };
        }

        // ── Contadores de negocio (helpers) ───────────────────────────────────

        public void RegistrarVenta(decimal monto)
        {
            Increment(MetricNames.VentasTotal);
            RecordHistogram(MetricNames.VentasMonto, (double)monto);
        }

        public void RegistrarLoginExitoso()  => Increment(MetricNames.LoginExitoso);
        public void RegistrarLoginFallido()  => Increment(MetricNames.LoginFallido);
        public void RegistrarErrorSistema()  => Increment(MetricNames.ErroresSistema);

        public void ActualizarUsuariosActivos(int count)
            => SetGauge(MetricNames.UsuariosActivos, count);

        // ── Privado ───────────────────────────────────────────────────────────

        private void Enqueue(MetricPoint point)
        {
            _recentPoints.Enqueue(point);
            while (_recentPoints.Count > MaxRecentPoints)
                _recentPoints.TryDequeue(out _);
        }

        private sealed class DurationMeasurement : IDisposable
        {
            private readonly MetricsService _metrics;
            private readonly string         _metricName;
            private readonly Dictionary<string, string>? _tags;
            private readonly Stopwatch      _sw;
            private bool _disposed;

            public DurationMeasurement(MetricsService metrics, string metricName,
                Dictionary<string, string>? tags)
            {
                _metrics    = metrics;
                _metricName = metricName;
                _tags       = tags;
                _sw         = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _sw.Stop();
                _metrics.RecordHistogram(_metricName, _sw.ElapsedMilliseconds, _tags);
            }
        }
    }

    /// <summary>Nombres de métricas estandarizados.</summary>
    public static class MetricNames
    {
        public const string VentasTotal    = "ventas.total";
        public const string VentasMonto    = "ventas.monto_ms";
        public const string LoginExitoso   = "auth.login.exitoso";
        public const string LoginFallido   = "auth.login.fallido";
        public const string ErroresSistema = "sistema.errores";
        public const string UsuariosActivos= "sistema.usuarios_activos";
        public const string QueryDuracion  = "db.query.duracion_ms";
        public const string CacheHit       = "cache.hit";
        public const string CacheMiss      = "cache.miss";
    }

    public class MetricsSummary
    {
        public Dictionary<string, long>             Counters   { get; set; } = new();
        public Dictionary<string, double>           Gauges     { get; set; } = new();
        public Dictionary<string, HistogramSummary> Histograms { get; set; } = new();
    }

    public class HistogramSummary
    {
        public int    Count { get; set; }
        public double Min   { get; set; }
        public double Max   { get; set; }
        public double Mean  { get; set; }
        public double P50   { get; set; }
        public double P95   { get; set; }
        public double P99   { get; set; }
    }
}
