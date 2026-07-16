using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using SistemaVentas.Commands;
using SistemaVentas.Models;
using SistemaVentas.Services;

namespace SistemaVentas.ViewModels
{
    /// <summary>
    /// ViewModel para gestión de usuarios
    /// </summary>
    public class UsuariosViewModel : BaseViewModel
    {
        private readonly UsuariosService _usuarios;
        private readonly AuditoriaService _auditoria;

        private string _nombreNuevo;
        private string _numeroNuevo;
        private string _rolSeleccionado;
        private bool _cargando;
        private ObservableCollection<Usuario> _usuariosActivos;

        public ICommand CargarUsuariosCommand { get; }
        public ICommand CrearUsuarioCommand { get; }
        public ICommand EliminarUsuarioCommand { get; }

        public UsuariosViewModel(
            UsuariosService usuarios,
            AuditoriaService auditoria)
        {
            _usuarios = usuarios;
            _auditoria = auditoria;
            _usuariosActivos = new ObservableCollection<Usuario>();

            CargarUsuariosCommand = new AsyncRelayCommand(CargarUsuariosAsync);
            CrearUsuarioCommand = new AsyncRelayCommand(CrearUsuarioAsync, PuedeCrear);
            EliminarUsuarioCommand = new AsyncRelayCommand<long>(EliminarUsuarioAsync);

            _rolSeleccionado = "Vendedor";
        }

        #region Propiedades

        public string NombreNuevo
        {
            get => _nombreNuevo;
            set => SetProperty(ref _nombreNuevo, value);
        }

        public string NumeroNuevo
        {
            get => _numeroNuevo;
            set => SetProperty(ref _numeroNuevo, value);
        }

        public string RolSeleccionado
        {
            get => _rolSeleccionado;
            set => SetProperty(ref _rolSeleccionado, value);
        }

        public bool Cargando
        {
            get => _cargando;
            set => SetProperty(ref _cargando, value);
        }

        public ObservableCollection<Usuario> UsuariosActivos
        {
            get => _usuariosActivos;
            set => SetProperty(ref _usuariosActivos, value);
        }

        #endregion

        #region Métodos

        public async Task CargarUsuariosAsync()
        {
            try
            {
                Cargando = true;
                var usuarios = await _usuarios.ObtenerVendedoresActivosAsync();
                UsuariosActivos = new ObservableCollection<Usuario>(usuarios);
            }
            catch (Exception ex)
            {
                ManejarError("Error Cargando Usuarios", ex);
            }
            finally
            {
                Cargando = false;
            }
        }

        private async Task CrearUsuarioAsync()
        {
            try
            {
                Cargando = true;

                var usuario = await _usuarios.CrearUsuarioAsync(
                    nombreCompleto: NombreNuevo,
                    numero: NumeroNuevo,
                    rol: RolSeleccionado
                );

                if (usuario != null)
                {
                    await _auditoria.RegistrarAsync(
                        accion: "CREAR_USUARIO",
                        modulo: "Usuarios",
                        detalle: $"Usuario creado: {NombreNuevo} ({RolSeleccionado})"
                    );

                    MostrarMensaje($"Usuario creado: {usuario.NombreCompleto}", "Éxito");
                    await CargarUsuariosAsync();
                    Limpiar();
                }
            }
            catch (Exception ex)
            {
                ManejarError("Error Creando Usuario", ex);
            }
            finally
            {
                Cargando = false;
            }
        }

        private async Task EliminarUsuarioAsync(long usuarioId)
        {
            if (MostrarConfirmacion(
                "¿Está seguro de eliminar este usuario?",
                "Confirmar",
                System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    Cargando = true;
                    await _usuarios.EliminarUsuarioAsync(usuarioId);
                    await _auditoria.RegistrarAsync(
                        accion: "ELIMINAR_USUARIO",
                        modulo: "Usuarios",
                        detalle: $"Usuario eliminado: ID {usuarioId}"
                    );
                    await CargarUsuariosAsync();
                }
                catch (Exception ex)
                {
                    ManejarError("Error Eliminando Usuario", ex);
                }
                finally
                {
                    Cargando = false;
                }
            }
        }

        private void Limpiar()
        {
            NombreNuevo = "";
            NumeroNuevo = "";
            RolSeleccionado = "Vendedor";
        }

        private bool PuedeCrear()
        {
            return !string.IsNullOrWhiteSpace(NombreNuevo) && 
                   !string.IsNullOrWhiteSpace(NumeroNuevo) && 
                   !Cargando;
        }

        #endregion
    }
}
