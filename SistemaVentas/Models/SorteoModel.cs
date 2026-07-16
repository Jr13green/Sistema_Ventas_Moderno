using System;
using System.Collections.Generic;

namespace SistemaVentas.Models
{
    /// <summary>
    /// Representa un sorteo configurado (ej: Diaria 11:00 AM)
    /// </summary>
    public class SorteoModel
    {
        public long Id { get; set; }
        public string Nombre { get; set; }
        public TimeSpan Hora { get; set; }
        public int MinutosCierreAntes { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Propiedades calculadas
        public TimeSpan HoraCierre => Hora.Subtract(TimeSpan.FromMinutes(MinutosCierreAntes));
    }
}