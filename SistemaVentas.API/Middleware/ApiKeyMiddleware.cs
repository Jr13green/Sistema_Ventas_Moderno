namespace SistemaVentas.API.Middleware
{
    /// <summary>
    /// Optional API key authentication middleware.
    /// Activated when the "Api:RequireApiKey" configuration is true.
    /// </summary>
    public sealed class ApiKeyMiddleware
    {
        private const string ApiKeyHeader = "X-API-Key";
        private const string ApiKeyQuery  = "api_key";

        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiKeyMiddleware> _logger;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration,
            ILogger<ApiKeyMiddleware> logger)
        {
            _next          = next;
            _configuration = configuration;
            _logger        = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip health and swagger endpoints
            var path = context.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            if (!bool.TryParse(_configuration["Api:RequireApiKey"], out var requireApiKey)
                || !requireApiKey)
            {
                await _next(context);
                return;
            }

            var providedKey = context.Request.Headers[ApiKeyHeader].FirstOrDefault()
                           ?? context.Request.Query[ApiKeyQuery].FirstOrDefault();

            var validKey = _configuration["Api:ApiKey"];

            if (string.IsNullOrEmpty(providedKey) || providedKey != validKey)
            {
                _logger.LogWarning("API key inválida desde {IP}",
                    context.Connection.RemoteIpAddress);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "API key inválida o no proporcionada" });
                return;
            }

            await _next(context);
        }
    }
}
