using System;
using System.Collections.Generic;

namespace SistemaVentas.Models
{
    /// <summary>
    /// Representa una venta completa con sus jugadas
    /// </summary>
    public class VentaModel
    {
        public long Id { get; set; }
        public string Codigo { get; set; }
        public long UsuarioId { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } // "Activa", "Anulada"
        public DateTime FechaCreacion { get; set; }

        // Propiedades de navegación
        public UsuarioModel Usuario { get; set; }
        public List<DetalleVentaModel> Detalles { get; set; } = new();

        // Propiedades calculadas
        public bool PuedeAnular => Estado == "Activa";
        public int CantidadJugadas => Detalles?.Count ?? 0;
    }
}