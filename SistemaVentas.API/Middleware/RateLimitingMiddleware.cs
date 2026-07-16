using System.Collections.Concurrent;

namespace SistemaVentas.API.Middleware
{
    /// <summary>
    /// Middleware de rate limiting por IP.
    /// Limita el número de requests por minuto por dirección IP.
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly int _maxRequestsPerMinute;

        // Contador por IP: (conteo, marca de tiempo del primer request en la ventana)
        private static readonly ConcurrentDictionary<string, (int Count, DateTime WindowStart)>
            _requestCounts = new();

        public RateLimitingMiddleware(
            RequestDelegate next,
            ILogger<RateLimitingMiddleware> logger,
            IConfiguration configuration)
        {
            _next   = next;
            _logger = logger;
            _maxRequestsPerMinute = int.TryParse(
                configuration["Api:RateLimitRequestsPerMinute"], out var limit)
                ? limit : 60;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (IsRateLimited(ip))
            {
                _logger.LogWarning("Rate limit excedido para IP: {IP}", ip);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = "60";
                await context.Response.WriteAsJsonAsync(new
                {
                    status  = 429,
                    error   = "Too Many Requests",
                    message = $"Límite de {_maxRequestsPerMinute} requests/minuto excedido."
                });
                return;
            }

            await _next(context);
        }

        private bool IsRateLimited(string ip)
        {
            var now = DateTime.UtcNow;

            var entry = _requestCounts.AddOrUpdate(ip,
                _ => (1, now),
                (_, existing) =>
                {
                    // Reiniciar ventana si pasó más de 1 minuto
                    if ((now - existing.WindowStart).TotalMinutes >= 1)
                        return (1, now);

                    return (existing.Count + 1, existing.WindowStart);
                });

            return entry.Count > _maxRequestsPerMinute;
        }
    }
}
