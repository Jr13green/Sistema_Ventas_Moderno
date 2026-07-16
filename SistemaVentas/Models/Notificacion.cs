using System;

namespace SistemaVentas.Models
{
    /// <summary>
    /// Notificación para panel principal.
    /// </summary>
    public class Notificacion
    {
        public long Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public bool Leida { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
