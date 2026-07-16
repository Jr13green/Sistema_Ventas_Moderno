using System.Threading.Tasks;

namespace SistemaVentas.Services
{
    /// <summary>
    /// Servicio base de configuración para binding MVVM.
    /// </summary>
    public class ConfiguracionService
    {
        public virtual Task InicializarCacheAsync() => Task.CompletedTask;

        public virtual string ObtenerNombreNegocio() => "Sistema de Ventas Diaria Familiar";
    }
}
