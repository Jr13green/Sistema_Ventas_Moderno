namespace SistemaVentas.Models
{
    /// <summary>
    /// Representa una jugada individual dentro de una venta
    /// </summary>
    public class DetalleVentaModel
    {
        public long Id { get; set; }
        public long VentaId { get; set; }
        public long SorteoDiarioId { get; set; }
        public string Numero { get; set; } // "00" a "99"
        public decimal Monto { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Propiedades de navegación
        public VentaModel Venta { get; set; }
        public SorteoDiarioModel SorteoDiario { get; set; }

        // Propiedades calculadas
        public string NumeroFormato => Numero?.PadLeft(2, '0') ?? "--";
    }
}