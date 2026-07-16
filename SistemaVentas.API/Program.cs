using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SistemaVentas.API.Middleware;
using SistemaVentas.API.Swagger;
using SistemaVentas.Caching;
using SistemaVentas.Monitoring;
using SistemaVentas.Security;
using SistemaVentas.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Servicios del dominio ─────────────────────────────────────────────────────
builder.Services.AddSingleton<BaseDatosService>();
builder.Services.AddSingleton<VentasService>();
builder.Services.AddSingleton<SorteosService>();
builder.Services.AddSingleton<CajaService>();
builder.Services.AddSingleton<ReportesService>();
builder.Services.AddSingleton<UsuariosService>();
builder.Services.AddSingleton<AuditoriaService>();
builder.Services.AddSingleton<ConfiguracionService>();

// ── Caching ───────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.AddSingleton<CacheStrategies>();

// ── Seguridad ─────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<EncryptionService>();
builder.Services.AddSingleton<InputValidator>();

// ── Monitoreo ─────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<MetricsService>();
builder.Services.AddSingleton<HealthCheckService>();
builder.Services.AddSingleton<ApplicationInsightsConfig>();

// ── Controladores y Swagger ───────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(SwaggerConfig.Configure);

// ── JWT ───────────────────────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["Security:JwtSecretKey"]
    ?? throw new InvalidOperationException("JWT secret key not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidIssuer              = "SistemaVentas",
            ValidateAudience         = true,
            ValidAudience            = "SistemaVentasAPI",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("SistemaVentasPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Api:AllowedOrigins")
                                   .Get<string[]>() ?? [];
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema Ventas API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("SistemaVentasPolicy");
app.UseHttpsRedirection();

if (bool.TryParse(builder.Configuration["Api:EnableRateLimiting"], out var rateLimit) && rateLimit)
    app.UseMiddleware<RateLimitingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ── Health endpoint público ───────────────────────────────────────────────────
app.MapGet("/health", async (HealthCheckService hc) =>
{
    var report = await hc.CheckAsync();
    var statusCode = report.Saludable ? 200 : 503;
    return Results.Json(report, statusCode: statusCode);
});

app.Run();
