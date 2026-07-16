using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SistemaVentas.Caching;
using SistemaVentas.Security;
using SistemaVentas.Services;

// Ejecutar todos los benchmarks
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAll();

// ── Benchmarks de Caché ────────────────────────────────────────────────────
[MemoryDiagnoser]
[SimpleJob]
[RPlotExporter]
public class CacheBenchmarks
{
    private ICacheService _cache = null!;
    private const string Key = "benchmark:test:key";

    [GlobalSetup]
    public void Setup()
    {
        _cache = new MemoryCacheService();
        _cache.SetAsync(Key, "valor_de_prueba", TimeSpan.FromMinutes(5)).Wait();
    }

    [Benchmark(Description = "Cache GET - Hit")]
    public async Task<(bool, string?)> CacheGet_Hit()
        => await _cache.GetAsync<string>(Key);

    [Benchmark(Description = "Cache GET - Miss")]
    public async Task<(bool, string?)> CacheGet_Miss()
        => await _cache.GetAsync<string>("key:inexistente:" + Guid.NewGuid());

    [Benchmark(Description = "Cache SET")]
    public async Task CacheSet()
        => await _cache.SetAsync("bench:set", "value", TimeSpan.FromMinutes(1));

    [Benchmark(Description = "Cache GetOrCreate")]
    public async Task<string> CacheGetOrCreate()
        => await _cache.GetOrCreateAsync(Key,
            () => Task.FromResult("valor_generado"),
            TimeSpan.FromMinutes(5));
}

// ── Benchmarks de Encriptación ─────────────────────────────────────────────
[MemoryDiagnoser]
[SimpleJob]
public class EncryptionBenchmarks
{
    private EncryptionService _encryption = null!;
    private string _plainText = "Datos sensibles del cliente 12345";
    private string _encryptedText = string.Empty;
    private string _hashedPassword = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _encryption    = new EncryptionService();
        _encryptedText = _encryption.Encrypt(_plainText);
        _hashedPassword = _encryption.HashPassword("MiContraseña123!");
    }

    [Benchmark(Description = "Encrypt - AES-256")]
    public string Encrypt()
        => _encryption.Encrypt(_plainText);

    [Benchmark(Description = "Decrypt - AES-256")]
    public string Decrypt()
        => _encryption.Decrypt(_encryptedText);

    [Benchmark(Description = "Hash Password - PBKDF2")]
    public string HashPassword()
        => _encryption.HashPassword("MiContraseña123!");

    [Benchmark(Description = "Verify Password - PBKDF2")]
    public bool VerifyPassword()
        => _encryption.VerifyPassword("MiContraseña123!", _hashedPassword);
}

// ── Benchmarks de Validación ───────────────────────────────────────────────
[MemoryDiagnoser]
[SimpleJob]
public class ValidationBenchmarks
{
    private InputValidator _validator = null!;

    [GlobalSetup]
    public void Setup()
    {
        _validator = new InputValidator();
    }

    [Benchmark(Description = "Validar Nombre")]
    public ValidationResult ValidarNombre()
        => _validator.ValidarNombre("Juan Carlos Pérez");

    [Benchmark(Description = "Validar Teléfono")]
    public ValidationResult ValidarTelefono()
        => _validator.ValidarTelefono("99887766");

    [Benchmark(Description = "Validar Número Lotería")]
    public ValidationResult ValidarNumeroLoteria()
        => _validator.ValidarNumeroLoteria("1234");

    [Benchmark(Description = "Validar Monto")]
    public ValidationResult ValidarMonto()
        => _validator.ValidarMonto(150.50m);

    [Benchmark(Description = "Sanitizar texto")]
    public string Sanitizar()
        => _validator.Sanitizar("  Texto  con  espacios  ");
}

// ── Benchmarks de Servicios ────────────────────────────────────────────────
[MemoryDiagnoser]
[SimpleJob]
public class ServiceBenchmarks
{
    private VentasService  _ventas   = null!;
    private SorteosService _sorteos  = null!;
    private CajaService    _caja     = null!;

    [GlobalSetup]
    public void Setup()
    {
        _ventas  = new VentasService();
        _sorteos = new SorteosService();
        _caja    = new CajaService();
    }

    [Benchmark(Description = "Obtener Total Ventas")]
    public async Task<decimal> ObtenerTotalVentas()
        => await _ventas.ObtenerTotalVentasActivasPorFechaAsync(DateTime.Today);

    [Benchmark(Description = "Obtener Sorteos del Día")]
    public async Task<List<SistemaVentas.Models.SorteoDiario>> ObtenerSorteosDia()
        => await _sorteos.ObtenerSorteosDiarioAsync(DateTime.Today);

    [Benchmark(Description = "Obtener Saldo Caja")]
    public async Task<decimal> ObtenerSaldoCaja()
        => await _caja.ObtenerSaldoCajaAsync(DateTime.Today);

    [Benchmark(Description = "Crear Venta - Jugadas válidas")]
    public async Task<(bool, string, long)> CrearVenta()
    {
        var jugadas = new List<(long sorteoDiarioId, string numero, decimal monto)>
        {
            (1L, "1234", 10m),
            (2L, "5678", 20m)
        };
        return await _ventas.CrearVentaAsync(1L, jugadas);
    }
}
