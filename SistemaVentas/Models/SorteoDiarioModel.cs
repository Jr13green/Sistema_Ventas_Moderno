using System;

namespace SistemaVentas.Models
{
    /// <summary>
    /// Representa la instancia diaria de un sorteo
    /// </summary>
    public class SorteoDiarioModel
    {
        public long Id { get; set; }
        public long SorteoId { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } // "Abierto", "Cerrado", "Pendiente", "Finalizado"
        public string Resultado { get; set; } // Número ganador "00"-"99"
        public DateTime? FechaResultado { get; set; }

        // Propiedades de navegación
        public SorteoModel Sorteo { get; set; }
    }
}