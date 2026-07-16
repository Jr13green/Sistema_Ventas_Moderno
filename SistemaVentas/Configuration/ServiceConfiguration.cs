using System;
using SistemaVentas.Caching;
using SistemaVentas.Monitoring;
using SistemaVentas.Security;
using SistemaVentas.Services;
// JwtAuthService lives in SistemaVentas.API project (requires Microsoft.IdentityModel packages)

namespace SistemaVentas.Configuration
{
    /// <summary>
    /// Configuración centralizada de servicios para FASE 4.
    /// Facilita el registro en el contenedor de dependencias (DI).
    /// </summary>
    public class AppSettings
    {
        public DatabaseSettings   Database   { get; set; } = new();
        public CacheSettings      Cache      { get; set; } = new();
        public SecuritySettings   Security   { get; set; } = new();
        public MonitoringSettings Monitoring { get; set; } = new();
        public ApiSettings        Api        { get; set; } = new();
        public BackupSettings     Backup     { get; set; } = new();
    }

    public class DatabaseSettings
    {
        public string ConnectionString       { get; set; } = "Data Source=SistemaVentas.db;Journal Mode=WAL;";
        public int    CommandTimeoutSeconds  { get; set; } = 30;
        public bool   EnableWAL             { get; set; } = true;
    }

    public class CacheSettings
    {
        public string Provider          { get; set; } = "Memory";
        public int    DefaultTtlSeconds { get; set; } = 300;
        public int    MaxSizeItems      { get; set; } = 1000;
    }

    public class SecuritySettings
    {
        public string JwtSecretKey          { get; set; } = string.Empty;
        public int    JwtExpiryMinutes      { get; set; } = 60;
        public string EncryptionKeyBase64   { get; set; } = string.Empty;
        public bool   EnableInputValidation { get; set; } = true;
    }

    public class MonitoringSettings
    {
        public string ApplicationInsightsConnectionString { get; set; } = string.Empty;
        public bool   EnableHealthChecks                  { get; set; } = true;
        public string HealthCheckPath                     { get; set; } = "/health";
    }

    public class ApiSettings
    {
        public string BaseUrl                    { get; set; } = "http://localhost:5000";
        public bool   EnableSwagger              { get; set; } = true;
        public bool   EnableRateLimiting         { get; set; } = true;
        public int    RateLimitRequestsPerMinute { get; set; } = 60;
    }

    public class BackupSettings
    {
        public bool   Enabled         { get; set; } = true;
        public string BackupPath      { get; set; } = "Backups/";
        public string DailyBackupTime { get; set; } = "02:00";
        public int    RetentionDays   { get; set; } = 30;
    }

    /// <summary>
    /// Factoría de servicios de FASE 4.
    /// Permite crear los servicios principales con la configuración apropiada.
    /// </summary>
    public class ServiceConfiguration
    {
        private readonly AppSettings _settings;

        public ServiceConfiguration(AppSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>Crea el servicio de caché según la configuración.</summary>
        public ICacheService CreateCacheService()
        {
            // Por ahora solo Memory; en el futuro: agregar RedisCacheService
            return new MemoryCacheService();
        }

        /// <summary>Crea el servicio de encriptación.</summary>
        public EncryptionService CreateEncryptionService()
        {
            var key = string.IsNullOrWhiteSpace(_settings.Security.EncryptionKeyBase64)
                ? null
                : _settings.Security.EncryptionKeyBase64;
            return new EncryptionService(key);
        }

        // Note: CreateJwtService() is defined in SistemaVentas.API (requires JWT NuGet packages)

        /// <summary>Crea el validador de entradas.</summary>
        public InputValidator CreateInputValidator()
            => new InputValidator();

        /// <summary>Crea el servicio de métricas.</summary>
        public MetricsService CreateMetricsService()
            => new MetricsService();

        /// <summary>Crea el servicio de health checks.</summary>
        public HealthCheckService CreateHealthCheckService(BaseDatosService baseDatos)
            => new HealthCheckService(baseDatos);
    }
}
