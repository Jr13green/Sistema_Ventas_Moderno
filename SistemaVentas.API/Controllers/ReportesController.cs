using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVentas.Caching;
using SistemaVentas.Services;

namespace SistemaVentas.API.Controllers
{
    /// <summary>Endpoints para generación de reportes.</summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly ReportesService _reportes;
        private readonly ICacheService   _cache;
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(
            ReportesService reportes,
            ICacheService cache,
            ILogger<ReportesController> logger)
        {
            _reportes = reportes;
            _cache    = cache;
            _logger   = logger;
        }

        /// <summary>Obtiene el resumen del período especificado.</summary>
        [HttpGet("resumen")]
        [ProducesResponseType(typeof(ResumenPeriodoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetResumen(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin)
        {
            if (fechaInicio > fechaFin)
                return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin.");
            if ((fechaFin - fechaInicio).TotalDays > 366)
                return BadRequest("El rango máximo es de 366 días.");

            var cacheKey = CacheKeys.Format(CacheKeys.ResumenPeriodo,
                fechaInicio.ToString("yyyy-MM-dd"),
                fechaFin.ToString("yyyy-MM-dd"));

            var (found, cached) = await _cache.GetAsync<ResumenPeriodoResponse>(cacheKey);
            if (found && cached is not null)
                return Ok(cached);

            var (ventas, premios, ganancia, transacciones) =
                await _reportes.ObtenerResumenPeriodoAsync(fechaInicio, fechaFin);

            var respuesta = new ResumenPeriodoResponse
            {
                FechaInicio   = fechaInicio,
                FechaFin      = fechaFin,
                TotalVentas   = ventas,
                TotalPremios  = premios,
                Ganancia      = ganancia,
                Transacciones = transacciones
            };

            await _cache.SetAsync(cacheKey, respuesta,
                TimeSpan.FromSeconds(CacheKeys.TtlLargo));

            return Ok(respuesta);
        }

        /// <summary>Obtiene los top vendedores del período.</summary>
        [HttpGet("top-vendedores")]
        [ProducesResponseType(typeof(IEnumerable<TopVendedorItem>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTopVendedores(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin,
            [FromQuery] int top = 10)
        {
            if (fechaInicio > fechaFin)
                return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin.");
            if (top < 1 || top > 100)
                top = 10;

            var cacheKey = CacheKeys.Format(CacheKeys.TopVendedores,
                fechaInicio.ToString("yyyy-MM-dd"),
                fechaFin.ToString("yyyy-MM-dd"));

            var (found, cached) = await _cache.GetAsync<List<TopVendedorItem>>(cacheKey);
            if (found && cached is not null)
                return Ok(cached.Take(top));

            var vendedores = await _reportes.ObtenerTopVendedoresConIdAsync(fechaInicio, fechaFin, top);
            var items = vendedores
                .Select(v => new TopVendedorItem
                {
                    UsuarioId = v.usuarioId,
                    Nombre    = v.nombre,
                    Total     = v.total,
                    Ventas    = v.ventas
                })
                .ToList();

            await _cache.SetAsync(cacheKey, items, TimeSpan.FromSeconds(CacheKeys.TtlLargo));

            return Ok(items);
        }

        /// <summary>Obtiene el listado de ganadores del período.</summary>
        [HttpGet("ganadores")]
        [ProducesResponseType(typeof(IEnumerable<GanadorItem>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGanadores(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin)
        {
            if (fechaInicio > fechaFin)
                return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin.");

            var ganadores = await _reportes.ObtenerGanadoresAsync(fechaInicio, fechaFin);
            var items = ganadores.Select(g => new GanadorItem
            {
                SorteoNombre = g.sorteoNombre,
                Fecha        = g.fecha,
                Numero       = g.numero,
                Monto        = g.monto,
                Vendedor     = g.vendedor
            });

            return Ok(items);
        }
    }

    public sealed record ResumenPeriodoResponse
    {
        public DateTime FechaInicio   { get; init; }
        public DateTime FechaFin      { get; init; }
        public decimal  TotalVentas   { get; init; }
        public decimal  TotalPremios  { get; init; }
        public decimal  Ganancia      { get; init; }
        public int      Transacciones { get; init; }
    }

    public sealed record TopVendedorItem
    {
        public long    UsuarioId { get; init; }
        public string  Nombre    { get; init; } = string.Empty;
        public decimal Total     { get; init; }
        public int     Ventas    { get; init; }
    }

    public sealed record GanadorItem
    {
        public string   SorteoNombre { get; init; } = string.Empty;
        public DateTime Fecha        { get; init; }
        public string   Numero       { get; init; } = string.Empty;
        public decimal  Monto        { get; init; }
        public string   Vendedor     { get; init; } = string.Empty;
    }
}
