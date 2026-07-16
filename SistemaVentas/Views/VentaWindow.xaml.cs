using System.Windows;
using SistemaVentas.ViewModels;
using SistemaVentas.Services;

namespace SistemaVentas.Views
{
    public partial class VentaWindow : Window
    {
        public VentaWindow(
            VentasService ventasService,
            SorteosService sorteosService,
            long usuarioId)
        {
            InitializeComponent();

            var viewModel = new VentaViewModel(ventasService, sorteosService, usuarioId);
            this.DataContext = viewModel;

            this.Loaded += async (s, e) => await viewModel.InicializarAsync();
        }
    }
}
