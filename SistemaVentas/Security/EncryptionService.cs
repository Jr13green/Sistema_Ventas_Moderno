using System;
using System.Security.Cryptography;
using System.Text;

namespace SistemaVentas.Security
{
    /// <summary>
    /// Servicio de encriptación para datos sensibles.
    /// Usa AES-256-CBC para encriptación simétrica y
    /// SHA-256 para hashing de contraseñas.
    /// </summary>
    public class EncryptionService
    {
        // La clave se obtiene de configuración en producción.
        // En desarrollo se usa una clave por defecto para facilitar las pruebas.
        private readonly byte[] _key;
        private const int KeySize  = 32; // 256 bits
        private const int IvSize   = 16; // 128 bits (tamaño de bloque AES)
        private const int Iterations = 100_000;

        public EncryptionService(string? base64Key = null)
        {
            if (!string.IsNullOrWhiteSpace(base64Key))
            {
                _key = Convert.FromBase64String(base64Key);
                if (_key.Length != KeySize)
                    throw new ArgumentException($"La clave debe ser de {KeySize} bytes (256 bits).");
            }
            else
            {
                // Clave determinista para desarrollo (NO usar en producción)
                _key = DeriveKey("SistemaVentasDevKey2026!", "SistemaVentasSalt");
            }
        }

        /// <summary>
        /// Encripta un texto plano y retorna Base64 (IV + CipherText).
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes  = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Concatenar IV + CipherText y codificar en Base64
            var result = new byte[IvSize + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, IvSize);
            Buffer.BlockCopy(cipherBytes, 0, result, IvSize, cipherBytes.Length);
            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// Desencripta un texto cifrado en Base64 (IV + CipherText).
        /// </summary>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            var fullBytes   = Convert.FromBase64String(cipherText);
            var iv          = new byte[IvSize];
            var cipherBytes = new byte[fullBytes.Length - IvSize];

            Buffer.BlockCopy(fullBytes, 0, iv, 0, IvSize);
            Buffer.BlockCopy(fullBytes, IvSize, cipherBytes, 0, cipherBytes.Length);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV  = iv;

            using var decryptor  = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }

        /// <summary>
        /// Genera un hash seguro para contraseñas usando PBKDF2-SHA256.
        /// El resultado incluye el salt embebido.
        /// </summary>
        public string HashPassword(string password)
        {
            var salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            var hash = new Rfc2898DeriveBytes(
                password, salt, Iterations, HashAlgorithmName.SHA256);

            var hashBytes = hash.GetBytes(32);
            var result    = new byte[48];   // 16 salt + 32 hash
            Buffer.BlockCopy(salt, 0, result, 0, 16);
            Buffer.BlockCopy(hashBytes, 0, result, 16, 32);
            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// Verifica una contraseña contra un hash almacenado.
        /// </summary>
        public bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                var hashBytes = Convert.FromBase64String(storedHash);
                if (hashBytes.Length != 48)
                    return false;

                var salt = new byte[16];
                Buffer.BlockCopy(hashBytes, 0, salt, 0, 16);

                var pbkdf2    = new Rfc2898DeriveBytes(
                    password, salt, Iterations, HashAlgorithmName.SHA256);
                var testHash  = pbkdf2.GetBytes(32);

                // Comparación en tiempo constante para evitar timing attacks
                return CryptographicOperations.FixedTimeEquals(
                    testHash,
                    hashBytes.AsSpan(16, 32));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Genera una clave aleatoria en Base64 para uso en configuración.
        /// </summary>
        public static string GenerateKey()
        {
            var key = new byte[KeySize];
            RandomNumberGenerator.Fill(key);
            return Convert.ToBase64String(key);
        }

        private static byte[] DeriveKey(string password, string salt)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                Encoding.UTF8.GetBytes(salt),
                Iterations,
                HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(KeySize);
        }
    }
}
