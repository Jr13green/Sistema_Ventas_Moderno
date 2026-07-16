using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVentas.Caching;
using SistemaVentas.Security;
using SistemaVentas.Services;

namespace SistemaVentas.API.Controllers
{
    /// <summary>Endpoints para gestión de ventas.</summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class VentasController : ControllerBase
    {
        private readonly VentasService   _ventas;
        private readonly ICacheService   _cache;
        private readonly CacheStrategies _cacheStrategies;
        private readonly InputValidator  _validator;
        private readonly ILogger<VentasController> _logger;

        public VentasController(
            VentasService ventas,
            ICacheService cache,
            CacheStrategies cacheStrategies,
            InputValidator validator,
            ILogger<VentasController> logger)
        {
            _ventas          = ventas;
            _cache           = cache;
            _cacheStrategies = cacheStrategies;
            _validator       = validator;
            _logger          = logger;
        }

        /// <summary>Obtiene el total de ventas activas para una fecha.</summary>
        [HttpGet("total")]
        [ProducesResponseType(typeof(TotalVentasResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTotal([FromQuery] DateTime? fecha = null)
        {
            var fechaConsulta = fecha?.Date ?? DateTime.Today;
            var cacheKey = CacheKeys.Format(CacheKeys.TotalVentasFecha,
                fechaConsulta.ToString("yyyy-MM-dd"));

            var (found, cached) = await _cache.GetAsync<decimal>(cacheKey);
            if (found)
                return Ok(new TotalVentasResponse { Fecha = fechaConsulta, Total = cached });

            var total = await _ventas.ObtenerTotalVentasActivasPorFechaAsync(fechaConsulta);
            await _cache.SetAsync(cacheKey, total,
                TimeSpan.FromSeconds(CacheKeys.TtlCorto));

            return Ok(new TotalVentasResponse { Fecha = fechaConsulta, Total = total });
        }

        /// <summary>Crea una nueva venta con sus jugadas.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(CrearVentaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CrearVentaRequest request)
        {
            var jugadas = request.Jugadas
                .Select(j => (j.SorteoDiarioId, j.Numero, j.Monto))
                .ToList();

            var validacion = _validator.ValidarVenta(request.UsuarioId, jugadas);
            if (!validacion.EsValido)
                return BadRequest(validacion.Mensaje);

            var (exito, mensaje, ventaId) = await _ventas.CrearVentaAsync(
                request.UsuarioId, jugadas);

            if (!exito)
                return BadRequest(mensaje);

            await _cacheStrategies.InvalidarVentasAsync(DateTime.Today, request.UsuarioId);

            _logger.LogInformation("Venta {VentaId} creada para usuario {UsuarioId}",
                ventaId, request.UsuarioId);

            return CreatedAtAction(nameof(GetTotal),
                new { fecha = DateTime.Today },
                new CrearVentaResponse { VentaId = ventaId, Mensaje = mensaje });
        }
    }

    public sealed record TotalVentasResponse
    {
        public DateTime Fecha { get; init; }
        public decimal  Total { get; init; }
    }

    public sealed record JugadaRequest(long SorteoDiarioId, string Numero, decimal Monto);

    public sealed record CrearVentaRequest
    {
        public long           UsuarioId { get; init; }
        public List<JugadaRequest> Jugadas { get; init; } = new();
    }

    public sealed record CrearVentaResponse
    {
        public long   VentaId { get; init; }
        public string Mensaje { get; init; } = string.Empty;
    }
}
