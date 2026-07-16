using System;

namespace SistemaVentas.Helpers
{
    /// <summary>
    /// Funciones para validaciones
    /// </summary>
    public static class ValidationHelper
    {
        public static bool EsNumeroValido(string numero)
        {
            if (string.IsNullOrEmpty(numero) || numero.Length != 2)
                return false;

            return int.TryParse(numero, out int num) && num >= 0 && num <= 99;
        }

        public static bool EsMontoValido(decimal monto)
        {
            return monto > 0;
        }

        public static bool EsCodigoValido(string codigo)
        {
            return !string.IsNullOrWhiteSpace(codigo) && codigo.Length >= 3;
        }

        public static bool EsNombreUsuarioValido(string nombre)
        {
            return !string.IsNullOrWhiteSpace(nombre) && nombre.Length >= 3;
        }
    }
}