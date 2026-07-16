namespace SistemaVentas.Models
{
    /// <summary>
    /// Modelo de usuario utilizado por los ViewModels MVVM
    /// </summary>
    public class Usuario
    {
        public long Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Rol { get; set; } = "Vendedor";
        public bool Activo { get; set; } = true;
    }
}
