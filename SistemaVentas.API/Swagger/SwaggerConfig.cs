using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SistemaVentas.API.Swagger
{
    /// <summary>
    /// Configuración de Swagger / OpenAPI para la API de Sistema de Ventas.
    /// </summary>
    public static class SwaggerConfig
    {
        public static void Configure(SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "Sistema de Ventas API",
                Version     = "v1",
                Description = "API REST para el Sistema de Ventas Diaria Familiar. " +
                              "Proporciona endpoints para ventas, sorteos, caja y reportes.",
                Contact = new OpenApiContact
                {
                    Name  = "Soporte Sistema Ventas",
                    Email = "soporte@sistemaventas.local"
                },
                License = new OpenApiLicense
                {
                    Name = "Licencia Privada"
                }
            });

            // Seguridad JWT en Swagger UI
            var jwtScheme = new OpenApiSecurityScheme
            {
                Name         = "Authorization",
                In           = ParameterLocation.Header,
                Type         = SecuritySchemeType.Http,
                Scheme       = "bearer",
                BearerFormat = "JWT",
                Description  = "Ingrese el token JWT con formato: ******"
            };

            options.AddSecurityDefinition("Bearer", jwtScheme);

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Incluir comentarios XML si existen
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        }
    }
}
