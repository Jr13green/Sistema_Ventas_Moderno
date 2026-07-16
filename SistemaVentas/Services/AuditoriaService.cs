using System.Threading.Tasks;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio base de auditoría para operaciones MVVM.
    /// </summary>
    public class AuditoriaService
    {
        public Task RegistrarAsync(string accion, string modulo, string detalle)
        {
            return Task.CompletedTask;
        }
    }
}
