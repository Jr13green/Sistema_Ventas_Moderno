namespace SistemaVentas.API.Middleware
{
    /// <summary>
    /// Adds security headers: CSP, HSTS, X-Frame-Options, X-Content-Type-Options, etc.
    /// </summary>
    public sealed class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var headers = context.Response.Headers;

            // Content-Security-Policy
            headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data:; " +
                "font-src 'self'; " +
                "connect-src 'self'; " +
                "frame-ancestors 'none'; " +
                "form-action 'self'; " +
                "base-uri 'self'";

            // HTTP Strict Transport Security (HSTS)
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";

            // Prevent clickjacking
            headers["X-Frame-Options"] = "DENY";

            // Prevent MIME type sniffing
            headers["X-Content-Type-Options"] = "nosniff";

            // Referrer policy
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Permissions policy
            headers["Permissions-Policy"] =
                "accelerometer=(), camera=(), geolocation=(), gyroscope=(), " +
                "magnetometer=(), microphone=(), payment=(), usb=()";

            // Remove server info
            headers.Remove("Server");
            headers.Remove("X-Powered-By");

            await _next(context);
        }
    }
}
