using System;

namespace SistemaVentas.Models
{
    /// <summary>
    /// Modelo simplificado para binding en ViewModels MVVM.
    /// </summary>
    public class SorteoDiario
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Today;
        public string Estado { get; set; } = "Abierto";
    }
}
