using System;

namespace SistemaVentas.Helpers
{
    /// <summary>
    /// Funciones para trabajar con fechas/horas
    /// </summary>
    public static class DateTimeHelper
    {
        public static (DateTime inicio, DateTime fin) ObtenerSemanaActual()
        {
            DateTime hoy = DateTime.Today;
            int diferencia = ((int)hoy.DayOfWeek + 6) % 7; // Lunes = 0
            DateTime inicio = hoy.AddDays(-diferencia);
            return (inicio, inicio.AddDays(6));
        }

        public static DateTime ObtenerInicioDelMes()
        {
            return new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }

        public static DateTime ObtenerFinDelMes()
        {
            return ObtenerInicioDelMes().AddMonths(1).AddDays(-1);
        }

        public static string FormatearTiempo(TimeSpan tiempo)
        {
            if (tiempo.TotalSeconds < 0)
                return "00:00:00";

            return $"{(int)tiempo.TotalHours:00}:{tiempo.Minutes:00}:{tiempo.Seconds:00}";
        }

        public static TimeSpan ObtenerTiempoRestante(DateTime fechaDestino)
        {
            return fechaDestino - DateTime.Now;
        }
    }
}