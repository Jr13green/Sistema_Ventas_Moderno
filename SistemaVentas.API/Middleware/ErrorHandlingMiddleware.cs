using System.Net;
using System.Text.Json;

namespace SistemaVentas.API.Middleware
{
    /// <summary>
    /// Middleware de manejo global de errores.
    /// Captura excepciones no manejadas y retorna respuestas JSON estructuradas.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger)
        {
            _next   = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no manejado en {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, mensaje) = ex switch
            {
                ArgumentException           => (HttpStatusCode.BadRequest,       ex.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized,     "No autorizado."),
                KeyNotFoundException        => (HttpStatusCode.NotFound,         "Recurso no encontrado."),
                OperationCanceledException  => (HttpStatusCode.ServiceUnavailable, "Operación cancelada."),
                _                          => (HttpStatusCode.InternalServerError, "Error interno del servidor.")
            };

            context.Response.StatusCode = (int)statusCode;

            var response = new ErrorResponse
            {
                Status  = (int)statusCode,
                Error   = statusCode.ToString(),
                Message = mensaje,
                Path    = context.Request.Path,
                TraceId = context.TraceIdentifier
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }

    public sealed class ErrorResponse
    {
        public int    Status  { get; set; }
        public string Error   { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Path    { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;
    }
}
