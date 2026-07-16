using System;

namespace SistemaVentas.Models
{
    /// <summary>
    /// Representa un usuario del sistema (Admin, Vendedor)
    /// </summary>
    public class UsuarioModel
    {
        public long Id { get; set; }
        public string NombreCompleto { get; set; }
        public string NombreUsuario { get; set; }
        public string Contrasena { get; set; }
        public string Rol { get; set; } // "Propietario", "Administrador", "Vendedor"
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }

        public bool EsPropietario => Rol == "Propietario";
        public bool EsAdministrador => Rol == "Administrador" || EsPropietario;
    }
}