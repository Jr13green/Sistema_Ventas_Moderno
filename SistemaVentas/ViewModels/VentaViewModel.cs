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
    /// ViewModel para crear y gestionar ventas
    /// </summary>
    public class VentaViewModel : BaseViewModel
    {
        private readonly VentasService _ventasService;
        private readonly SorteosService _sorteosService;
        private readonly long _usuarioId;

        private decimal _montoIngresado;
        private string _numeroIngresado;
        private SorteoDiario _sorteoSeleccionado;
        private bool _cargando;
        private ObservableCollection<SorteoDiario> _sorteosDia;
        private ObservableCollection<DetalleVentaTemporal> _jugadasAgregadas;

        public ICommand AgregarJugadaCommand { get; }
        public ICommand EliminarJugadaCommand { get; }
        public ICommand GuardarVentaCommand { get; }
        public ICommand LimpiarCommand { get; }

        public VentaViewModel(
            VentasService ventasService,
            SorteosService sorteosService,
            long usuarioId)
        {
            _ventasService = ventasService;
            _sorteosService = sorteosService;
            _usuarioId = usuarioId;

            _sorteosDia = new ObservableCollection<SorteoDiario>();
            _jugadasAgregadas = new ObservableCollection<DetalleVentaTemporal>();

            AgregarJugadaCommand = new RelayCommand(AgregarJugada, PuedeAgregarJugada);
            EliminarJugadaCommand = new RelayCommand<DetalleVentaTemporal>(EliminarJugada);
            GuardarVentaCommand = new AsyncRelayCommand(GuardarVentaAsync, PuedeGuardar);
            LimpiarCommand = new RelayCommand(Limpiar);
        }

        #region Propiedades

        public decimal MontoIngresado
        {
            get => _montoIngresado;
            set => SetProperty(ref _montoIngresado, value);
        }

        public string NumeroIngresado
        {
            get => _numeroIngresado;
            set => SetProperty(ref _numeroIngresado, value);
        }

        public SorteoDiario SorteoSeleccionado
        {
            get => _sorteoSeleccionado;
            set => SetProperty(ref _sorteoSeleccionado, value);
        }

        public bool Cargando
        {
            get => _cargando;
            set => SetProperty(ref _cargando, value);
        }

        public ObservableCollection<SorteoDiario> SorteosDia
        {
            get => _sorteosDia;
            set => SetProperty(ref _sorteosDia, value);
        }

        public ObservableCollection<DetalleVentaTemporal> JugadasAgregadas
        {
            get => _jugadasAgregadas;
            set => SetProperty(ref _jugadasAgregadas, value);
        }

        public decimal TotalVenta
        {
            get
            {
                decimal total = 0;
                foreach (var jugada in _jugadasAgregadas)
                    total += jugada.Monto;
                return total;
            }
        }

        #endregion

        #region Métodos Públicos

        public async Task InicializarAsync()
        {
            try
            {
                Cargando = true;
                var sorteos = await _sorteosService.ObtenerSorteosDiarioAsync(DateTime.Today);
                SorteosDia = new ObservableCollection<SorteoDiario>(sorteos);
            }
            catch (Exception ex)
            {
                ManejarError("Error Inicializando", ex);
            }
            finally
            {
                Cargando = false;
            }
        }

        #endregion

        #region Métodos Privados

        private void AgregarJugada()
        {
            if (SorteoSeleccionado == null || MontoIngresado <= 0 || string.IsNullOrWhiteSpace(NumeroIngresado))
            {
                System.Windows.MessageBox.Show("Ingrese datos válidos", "Validación");
                return;
            }

            var jugada = new DetalleVentaTemporal
            {
                SorteoDiarioId = SorteoSeleccionado.Id,
                Sorteo = SorteoSeleccionado.Nombre,
                Numero = NumeroIngresado.PadLeft(2, '0'),
                Monto = MontoIngresado
            };

            JugadasAgregadas.Add(jugada);
            OnPropertyChanged(nameof(TotalVenta));

            // Limpiar campos
            MontoIngresado = 0;
            NumeroIngresado = "";
            SorteoSeleccionado = null;
        }

        private void EliminarJugada(DetalleVentaTemporal jugada)
        {
            if (jugada != null)
            {
                JugadasAgregadas.Remove(jugada);
                OnPropertyChanged(nameof(TotalVenta));
            }
        }

        private async Task GuardarVentaAsync()
        {
            try
            {
                Cargando = true;

                var jugadas = new System.Collections.Generic.List<(long, string, decimal)>();
                foreach (var jugada in JugadasAgregadas)
                    jugadas.Add((jugada.SorteoDiarioId, jugada.Numero, jugada.Monto));

                var (exito, mensaje, ventaId) = await _ventasService.CrearVentaAsync(_usuarioId, jugadas);

                if (exito)
                {
                    System.Windows.MessageBox.Show($"Venta guardada: {ventaId}", "Éxito");
                    Limpiar();
                }
                else
                {
                    System.Windows.MessageBox.Show(mensaje, "Error");
                }
            }
            catch (Exception ex)
            {
                ManejarError("Error Guardando", ex);
            }
            finally
            {
                Cargando = false;
            }
        }

        private void Limpiar()
        {
            JugadasAgregadas.Clear();
            MontoIngresado = 0;
            NumeroIngresado = "";
            SorteoSeleccionado = null;
            OnPropertyChanged(nameof(TotalVenta));
        }

        private bool PuedeAgregarJugada()
        {
            return SorteoSeleccionado != null && MontoIngresado > 0 && !string.IsNullOrWhiteSpace(NumeroIngresado);
        }

        private bool PuedeGuardar()
        {
            return JugadasAgregadas.Count > 0 && !Cargando;
        }

        #endregion
    }

    /// <summary>
    /// Modelo temporal para jugadas antes de guardar
    /// </summary>
    public class DetalleVentaTemporal
    {
        public long SorteoDiarioId { get; set; }
        public string Sorteo { get; set; }
        public string Numero { get; set; }
        public decimal Monto { get; set; }
    }
}
