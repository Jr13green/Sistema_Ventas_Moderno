using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SistemaVentas.API.Security
{
    /// <summary>
    /// Servicio de autenticación JWT para la API REST.
    /// Genera y valida tokens JWT con claims de usuario.
    /// </summary>
    public class JwtAuthService
    {
        private readonly string _issuer;
        private readonly string _audience;
        private readonly SymmetricSecurityKey _signingKey;
        private readonly TimeSpan _tokenExpiry;
        private readonly JwtSecurityTokenHandler _handler;

        public JwtAuthService(
            string secretKey,
            string issuer       = "SistemaVentas",
            string audience     = "SistemaVentasAPI",
            int expiryMinutes   = 60)
        {
            if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
                throw new ArgumentException("La clave JWT debe tener al menos 32 caracteres.");

            _issuer      = issuer;
            _audience    = audience;
            _tokenExpiry = TimeSpan.FromMinutes(expiryMinutes);
            _signingKey  = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            _handler     = new JwtSecurityTokenHandler();
        }

        /// <summary>Genera un token JWT para el usuario autenticado.</summary>
        public string GenerarToken(long usuarioId, string nombre, string rol)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,  usuarioId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, nombre),
                new Claim(ClaimTypes.Role,              rol),
                new Claim(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer:             _issuer,
                audience:           _audience,
                claims:             claims,
                notBefore:          DateTime.UtcNow,
                expires:            DateTime.UtcNow.Add(_tokenExpiry),
                signingCredentials: credentials);

            return _handler.WriteToken(token);
        }

        /// <summary>Genera un refresh token seguro de 64 bytes en Base64.</summary>
        public static string GenerarRefreshToken()
        {
            var bytes = new byte[64];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
