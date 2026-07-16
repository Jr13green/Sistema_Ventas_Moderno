using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaVentas.Models;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio base para notificaciones.
    /// </summary>
    public class NotificacionesService
    {
        public virtual Task<int> ObtenerConteoNoLeidasAsync()
        {
            return Task.FromResult(0);
        }

        public virtual Task<List<Notificacion>> ObtenerNotificacionesActivasAsync(int limite)
        {
            return Task.FromResult(new List<Notificacion>());
        }
    }
}
