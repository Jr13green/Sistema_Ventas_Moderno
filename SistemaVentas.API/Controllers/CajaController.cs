using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVentas.Caching;
using SistemaVentas.Services;

namespace SistemaVentas.API.Controllers
{
    /// <summary>Endpoints para gestión de caja.</summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class CajaController : ControllerBase
    {
        private readonly CajaService     _caja;
        private readonly ICacheService   _cache;
        private readonly CacheStrategies _cacheStrategies;
        private readonly ILogger<CajaController> _logger;

        public CajaController(
            CajaService caja,
            ICacheService cache,
            CacheStrategies cacheStrategies,
            ILogger<CajaController> logger)
        {
            _caja            = caja;
            _cache           = cache;
            _cacheStrategies = cacheStrategies;
            _logger          = logger;
        }

        /// <summary>Obtiene el saldo de caja para la fecha indicada.</summary>
        [HttpGet("saldo")]
        [ProducesResponseType(typeof(SaldoCajaResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSaldo([FromQuery] DateTime? fecha = null)
        {
            var fechaConsulta = fecha?.Date ?? DateTime.Today;

            var (found, cached) = await _cache.GetAsync<decimal>(CacheKeys.SaldoCaja);
            if (found && fechaConsulta == DateTime.Today)
                return Ok(new SaldoCajaResponse { Fecha = fechaConsulta, Saldo = cached });

            var saldo = await _caja.ObtenerSaldoCajaAsync(fechaConsulta);
            if (fechaConsulta == DateTime.Today)
                await _cache.SetAsync(CacheKeys.SaldoCaja, saldo,
                    TimeSpan.FromSeconds(CacheKeys.TtlCorto));

            return Ok(new SaldoCajaResponse { Fecha = fechaConsulta, Saldo = saldo });
        }

        /// <summary>Registra un movimiento de caja (ingreso o egreso).</summary>
        [HttpPost("movimiento")]
        [ProducesResponseType(typeof(MovimientoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegistrarMovimiento(
            [FromBody] MovimientoRequest request)
        {
            if (request.Monto <= 0)
                return BadRequest("El monto debe ser mayor que cero.");
            if (string.IsNullOrWhiteSpace(request.Tipo))
                return BadRequest("El tipo de movimiento es requerido.");

            var (exito, mensaje) = await _caja.RegistrarMovimientoAsync(
                request.Tipo, request.Monto, request.Descripcion ?? string.Empty);

            if (!exito)
                return BadRequest(mensaje);

            await _cacheStrategies.InvalidarCajaAsync();

            _logger.LogInformation("Movimiento de caja: {Tipo} L{Monto:N2}", request.Tipo, request.Monto);
            return CreatedAtAction(nameof(GetSaldo), null,
                new MovimientoResponse { Exito = true, Mensaje = mensaje });
        }
    }

    public sealed record SaldoCajaResponse
    {
        public DateTime Fecha { get; init; }
        public decimal  Saldo { get; init; }
    }

    public sealed record MovimientoRequest
    {
        public string  Tipo        { get; init; } = string.Empty;
        public decimal Monto       { get; init; }
        public string? Descripcion { get; init; }
    }

    public sealed record MovimientoResponse
    {
        public bool   Exito   { get; init; }
        public string Mensaje { get; init; } = string.Empty;
    }
}
