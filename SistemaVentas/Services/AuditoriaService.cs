using System.Threading.Tasks;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio base de auditoría para operaciones MVVM.
    /// </summary>
    public class AuditoriaService
    {
        public virtual Task RegistrarAsync(string accion, string modulo, string detalle)
        {
            return Task.CompletedTask;
        }
    }
}
