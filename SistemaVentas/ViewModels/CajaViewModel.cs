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
    /// ViewModel para gestión de caja
    /// </summary>
    public class CajaViewModel : BaseViewModel
    {
        private readonly CajaService _caja;
        private readonly AuditoriaService _auditoria;

        private decimal _saldoActual;
        private decimal _montoAjuste;
        private string _motivoAjuste;
        private string _tipoAjuste;
        private bool _cargando;
        private ObservableCollection<(string tipo, decimal monto, string motivo, DateTime fecha)> _movimientos;
        public static string[] TiposAjuste { get; } = { "Ingreso", "Egreso" };

        public ICommand AgregarAjusteCommand { get; }
        public ICommand CargarMovimientosCommand { get; }
        public ICommand LimpiarCommand { get; }

        public CajaViewModel(
            CajaService caja,
            AuditoriaService auditoria)
        {
            _caja = caja;
            _auditoria = auditoria;
            _movimientos = new ObservableCollection<(string, decimal, string, DateTime)>();

            AgregarAjusteCommand = new AsyncRelayCommand(AgregarAjusteAsync, PuedeAgregarAjuste);
            CargarMovimientosCommand = new AsyncRelayCommand(CargarMovimientosAsync);
            LimpiarCommand = new RelayCommand(Limpiar);

            _tipoAjuste = "Ingreso";
        }

        #region Propiedades

        public decimal SaldoActual
        {
            get => _saldoActual;
            set => SetProperty(ref _saldoActual, value);
        }

        public decimal MontoAjuste
        {
            get => _montoAjuste;
            set => SetProperty(ref _montoAjuste, value);
        }

        public string MotivoAjuste
        {
            get => _motivoAjuste;
            set => SetProperty(ref _motivoAjuste, value);
        }

        public string TipoAjuste
        {
            get => _tipoAjuste;
            set => SetProperty(ref _tipoAjuste, value);
        }

        public bool Cargando
        {
            get => _cargando;
            set => SetProperty(ref _cargando, value);
        }

        public ObservableCollection<(string, decimal, string, DateTime)> Movimientos
        {
            get => _movimientos;
            set => SetProperty(ref _movimientos, value);
        }

        #endregion

        #region Métodos

        public async Task InicializarAsync()
        {
            SaldoActual = await _caja.ObtenerSaldoCajaAsync(DateTime.Today);
            await CargarMovimientosAsync();
        }

        private async Task AgregarAjusteAsync()
        {
            try
            {
                Cargando = true;

                await _caja.RegistrarAjusteAsync(
                    tipo: TipoAjuste,
                    monto: MontoAjuste,
                    motivo: MotivoAjuste
                );

                await _auditoria.RegistrarAsync(
                    accion: "AJUSTE_CAJA",
                    modulo: "Caja",
                    detalle: $"{TipoAjuste} de L {MontoAjuste:N2}: {MotivoAjuste}"
                );

                MostrarMensaje("Ajuste registrado correctamente", "Éxito");
                await InicializarAsync();
                Limpiar();
            }
            catch (Exception ex)
            {
                ManejarError("Error Guardando Ajuste", ex);
            }
            finally
            {
                Cargando = false;
            }
        }

        private async Task CargarMovimientosAsync()
        {
            try
            {
                Cargando = true;
                var resumen = await _caja.ObtenerResumenCajaPorFechaAsync(DateTime.Today);
                SaldoActual = resumen.saldoFinal;
                // Aquí cargarías los movimientos del día
            }
            catch (Exception ex)
            {
                ManejarError("Error Cargando Movimientos", ex);
            }
            finally
            {
                Cargando = false;
            }
        }

        private void Limpiar()
        {
            MontoAjuste = 0;
            MotivoAjuste = "";
            TipoAjuste = "Ingreso";
        }

        private bool PuedeAgregarAjuste()
        {
            return MontoAjuste > 0 && !string.IsNullOrWhiteSpace(MotivoAjuste) && !Cargando;
        }

        #endregion
    }
}
