using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SistemaVentas.Security
{
    /// <summary>
    /// Servicio de validación de entradas para prevenir inyección
    /// y garantizar integridad de datos en todas las capas.
    /// </summary>
    public class InputValidator
    {
        // Regex compiladas para performance
        private static readonly Regex _soloNumeros       = new(@"^\d+$",            RegexOptions.Compiled);
        private static readonly Regex _soloLetrasNumeros = new(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\s]+$", RegexOptions.Compiled);
        private static readonly Regex _telefono          = new(@"^\d{8,15}$",       RegexOptions.Compiled);
        private static readonly Regex _sqlInjection      = new(
            @"('|--|;|/\*|\*/|xp_|EXEC|EXECUTE|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|UNION|SELECT\s)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // ── Validaciones generales ────────────────────────────────────────────

        public ValidationResult ValidarRequerido(string? valor, string campo, int maxLength = 100)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return ValidationResult.Fail($"El campo '{campo}' es requerido.");
            if (valor.Length > maxLength)
                return ValidationResult.Fail($"El campo '{campo}' no puede superar {maxLength} caracteres.");
            return ValidationResult.Ok();
        }

        public ValidationResult ValidarMonto(decimal monto, decimal min = 0.01m, decimal max = 9_999_999.99m)
        {
            if (monto < min)
                return ValidationResult.Fail($"El monto debe ser mayor o igual a {min:N2}.");
            if (monto > max)
                return ValidationResult.Fail($"El monto no puede superar {max:N2}.");
            return ValidationResult.Ok();
        }

        public ValidationResult ValidarFecha(DateTime fecha, DateTime? fechaMinima = null, DateTime? fechaMaxima = null)
        {
            var min = fechaMinima ?? DateTime.Today.AddYears(-10);
            var max = fechaMaxima ?? DateTime.Today.AddYears(1);

            if (fecha.Date < min.Date)
                return ValidationResult.Fail($"La fecha no puede ser anterior a {min:yyyy-MM-dd}.");
            if (fecha.Date > max.Date)
                return ValidationResult.Fail($"La fecha no puede ser posterior a {max:yyyy-MM-dd}.");
            return ValidationResult.Ok();
        }

        // ── Validaciones de dominio ────────────────────────────────────────────

        public ValidationResult ValidarNombre(string? nombre)
        {
            var req = ValidarRequerido(nombre, "Nombre", 100);
            if (!req.EsValido) return req;

            if (!_soloLetrasNumeros.IsMatch(nombre!))
                return ValidationResult.Fail("El nombre solo puede contener letras, números y espacios.");
            return ValidationResult.Ok();
        }

        public ValidationResult ValidarTelefono(string? numero)
        {
            var req = ValidarRequerido(numero, "Número de teléfono", 15);
            if (!req.EsValido) return req;

            if (!_telefono.IsMatch(numero!))
                return ValidationResult.Fail("El número de teléfono debe tener entre 8 y 15 dígitos.");
            return ValidationResult.Ok();
        }

        public ValidationResult ValidarNumeroLoteria(string? numero)
        {
            var req = ValidarRequerido(numero, "Número de lotería", 4);
            if (!req.EsValido) return req;

            if (!_soloNumeros.IsMatch(numero!))
                return ValidationResult.Fail("El número de lotería solo puede contener dígitos.");
            return ValidationResult.Ok();
        }

        public ValidationResult ValidarPassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return ValidationResult.Fail("La contraseña es requerida.");
            if (password.Length < 6)
                return ValidationResult.Fail("La contraseña debe tener al menos 6 caracteres.");
            if (password.Length > 128)
                return ValidationResult.Fail("La contraseña no puede superar 128 caracteres.");
            return ValidationResult.Ok();
        }

        // ── Sanitización ───────────────────────────────────────────────────────

        /// <summary>
        /// Sanitiza una cadena para prevenir inyección SQL.
        /// Usar SIEMPRE con parámetros en las queries; este método es una capa adicional.
        /// </summary>
        public ValidationResult ValidarNoSqlInjection(string? valor, string campo)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return ValidationResult.Ok();
            if (_sqlInjection.IsMatch(valor))
                return ValidationResult.Fail($"El campo '{campo}' contiene caracteres no permitidos.");
            return ValidationResult.Ok();
        }

        /// <summary>Elimina caracteres de control y espacios redundantes.</summary>
        public string Sanitizar(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            // Eliminar caracteres de control
            var resultado = new string(valor.Where(c => !char.IsControl(c)).ToArray());
            return resultado.Trim();
        }

        // ── Validación compuesta ──────────────────────────────────────────────

        public ValidationResult ValidarVenta(
            long usuarioId,
            IEnumerable<(long sorteoDiarioId, string numero, decimal monto)> jugadas)
        {
            if (usuarioId <= 0)
                return ValidationResult.Fail("El vendedor es inválido.");

            var jugadasList = jugadas?.ToList();
            if (jugadasList == null || jugadasList.Count == 0)
                return ValidationResult.Fail("Debe ingresar al menos una jugada.");
            if (jugadasList.Count > 50)
                return ValidationResult.Fail("No puede ingresar más de 50 jugadas por venta.");

            foreach (var (sorteoDiarioId, numero, monto) in jugadasList)
            {
                if (sorteoDiarioId <= 0)
                    return ValidationResult.Fail("El sorteo seleccionado es inválido.");

                var numResult = ValidarNumeroLoteria(numero);
                if (!numResult.EsValido) return numResult;

                var montoResult = ValidarMonto(monto);
                if (!montoResult.EsValido) return montoResult;
            }

            return ValidationResult.Ok();
        }
    }

    /// <summary>Resultado de una validación.</summary>
    public class ValidationResult
    {
        public bool EsValido { get; private set; }
        public string Mensaje { get; private set; }

        private ValidationResult(bool esValido, string mensaje)
        {
            EsValido = esValido;
            Mensaje  = mensaje;
        }

        public static ValidationResult Ok()   => new(true,  string.Empty);
        public static ValidationResult Fail(string mensaje) => new(false, mensaje);

        public override string ToString() => EsValido ? "OK" : Mensaje;
    }
}
