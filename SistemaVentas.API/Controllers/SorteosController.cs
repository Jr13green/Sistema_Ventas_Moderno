using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVentas.Caching;
using SistemaVentas.Models;
using SistemaVentas.Services;

namespace SistemaVentas.API.Controllers
{
    /// <summary>Endpoints para gestión de sorteos.</summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class SorteosController : ControllerBase
    {
        private readonly SorteosService  _sorteos;
        private readonly ICacheService   _cache;
        private readonly CacheStrategies _cacheStrategies;
        private readonly ILogger<SorteosController> _logger;

        public SorteosController(
            SorteosService sorteos,
            ICacheService cache,
            CacheStrategies cacheStrategies,
            ILogger<SorteosController> logger)
        {
            _sorteos         = sorteos;
            _cache           = cache;
            _cacheStrategies = cacheStrategies;
            _logger          = logger;
        }

        /// <summary>Obtiene los sorteos del día especificado.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SorteoDiario>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByFecha([FromQuery] DateTime? fecha = null)
        {
            var fechaConsulta = fecha?.Date ?? DateTime.Today;
            var cacheKey = CacheKeys.Format(CacheKeys.SorteosDia,
                fechaConsulta.ToString("yyyy-MM-dd"));

            var (found, cached) = await _cache.GetAsync<List<SorteoDiario>>(cacheKey);
            if (found && cached is not null)
                return Ok(cached);

            var sorteos = await _sorteos.ObtenerSorteosDiarioAsync(fechaConsulta);
            await _cache.SetAsync(cacheKey, sorteos,
                TimeSpan.FromSeconds(CacheKeys.TtlMedio));

            return Ok(sorteos);
        }

        /// <summary>Crea los sorteos del día actual.</summary>
        [HttpPost("crear-dia")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> CrearSorteosDia()
        {
            await _sorteos.CrearSorteosDelDiaAsync();
            await _cacheStrategies.InvalidarSorteosAsync(DateTime.Today);

            _logger.LogInformation("Sorteos del día {Fecha} creados", DateTime.Today.ToString("yyyy-MM-dd"));
            return NoContent();
        }

        /// <summary>Actualiza el estado de los sorteos del día.</summary>
        [HttpPost("actualizar-estados")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ActualizarEstados()
        {
            await _sorteos.ActualizarEstadosSorteosAsync();
            await _cacheStrategies.InvalidarSorteosAsync(DateTime.Today);
            return NoContent();
        }
    }
}
