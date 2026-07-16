using System;

namespace SistemaVentas.Helpers
{
    /// <summary>
    /// Funciones para formatear datos
    /// </summary>
    public static class FormatHelper
    {
        public static string FormatearHora(TimeSpan hora)
        {
            return DateTime.Today.Add(hora).ToString("hh:mm tt");
        }

        public static string FormatearFecha(DateTime fecha)
        {
            return fecha.ToString("dddd, dd 'de' MMMM 'de' yyyy",
                new System.Globalization.CultureInfo("es-ES"));
        }

        public static string FormatearFechaHora(DateTime fecha)
        {
            return fecha.ToString("dd/MM/yyyy · hh:mm:ss tt");
        }

        public static string FormatearMoneda(decimal monto, string simbolo = "L")
        {
            return $"{simbolo} {monto:N2}";
        }

        public static string FormatearNumero(string numero)
        {
            return numero?.PadLeft(2, '0') ?? "--";
        }
    }
}