using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaVentas.Models;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio base para sorteos usado por MVVM.
    /// </summary>
    public class SorteosService
    {
        public Task CrearSorteosDelDiaAsync() => Task.CompletedTask;

        public Task ActualizarEstadosSorteosAsync() => Task.CompletedTask;

        public Task<List<SorteoDiario>> ObtenerSorteosDiarioAsync(DateTime fecha)
        {
            var items = new List<SorteoDiario>
            {
                new SorteoDiario { Id = 1, Nombre = "Matutino", Fecha = fecha, Estado = "Abierto" },
                new SorteoDiario { Id = 2, Nombre = "Vespertino", Fecha = fecha, Estado = "Abierto" }
            };
            return Task.FromResult(items);
        }
    }
}
