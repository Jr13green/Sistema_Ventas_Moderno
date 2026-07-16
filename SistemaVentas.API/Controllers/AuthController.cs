using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVentas.API.Security;
using SistemaVentas.Caching;
using SistemaVentas.Security;
using SistemaVentas.Services;

namespace SistemaVentas.API.Controllers
{
    /// <summary>Controlador de autenticación JWT.</summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UsuariosService _usuarios;
        private readonly EncryptionService _encryption;
        private readonly InputValidator _validator;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UsuariosService usuarios,
            EncryptionService encryption,
            InputValidator validator,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _usuarios      = usuarios;
            _encryption    = encryption;
            _validator     = validator;
            _configuration = configuration;
            _logger        = logger;
        }

        /// <summary>Autentica al usuario y retorna un token JWT.</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDetail), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDetail), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var numVal = _validator.ValidarTelefono(request.Numero);
            if (!numVal.EsValido)
                return BadRequest(new ErrorDetail(numVal.Mensaje));

            var pwdVal = _validator.ValidarPassword(request.Password);
            if (!pwdVal.EsValido)
                return BadRequest(new ErrorDetail(pwdVal.Mensaje));

            try
            {
                var usuario = await _usuarios.AutenticarAsync(request.Numero, request.Password);
                if (usuario == null)
                    return Unauthorized(new ErrorDetail("Credenciales inválidas."));

                var secret = _configuration["Security:JwtSecretKey"];
                if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32)
                    return StatusCode(503, new ErrorDetail("Autenticación no configurada."));

                var jwtService = new JwtAuthService(secret);
                var token      = jwtService.GenerarToken(usuario.Id, usuario.NombreCompleto, "Vendedor");
                var refresh    = JwtAuthService.GenerarRefreshToken();

                _logger.LogInformation("Login exitoso: usuario {Id} ({Nombre})",
                    usuario.Id, usuario.NombreCompleto);

                return Ok(new LoginResponse
                {
                    Token        = token,
                    RefreshToken = refresh,
                    ExpiresIn    = 3600,
                    Nombre       = usuario.NombreCompleto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login para número {Numero}", request.Numero);
                return StatusCode(500, new ErrorDetail("Error interno de autenticación."));
            }
        }
    }

    public sealed record LoginRequest(string Numero, string Password);
    public sealed record LoginResponse
    {
        public string Token        { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
        public int    ExpiresIn    { get; init; }
        public string Nombre       { get; init; } = string.Empty;
    }
    public sealed record ErrorDetail(string Message);
}
