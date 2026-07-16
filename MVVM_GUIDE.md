# 🎨 FASE 2: MVVM Refactor - Guía de Integración

## 📊 Estructura MVVM Implementada

```
SistemaVentas/
├── ViewModels/
│   ├── BaseViewModel.cs          ✅ Clase base con INotifyPropertyChanged
│   ├── DashboardViewModel.cs     ✅ Lógica del Dashboard
│   ├── VentaViewModel.cs         ✅ Lógica de creación de ventas
│   └── [Otros ViewModels]
│
├── Commands/
│   ├── RelayCommand.cs           ✅ Comando síncrono
│   ├── RelayCommand<T>.cs        ✅ Comando genérico
│   ├── AsyncRelayCommand.cs      ✅ Comando asíncrono
│   └── AsyncRelayCommand<T>.cs   ✅ Comando genérico asíncrono
│
├── Views/
│   ├── Dashboard.xaml            📝 Necesita refactor
│   ├── Dashboard.xaml.cs         📝 Solo DataContext
│   ├── VentaWindow.xaml          📝 Necesita crear
│   └── VentaWindow.xaml.cs       📝 Necesita crear
│
└── Services/
    └── [Ya creados en FASE 1]
```

---

## 🔄 Flujo MVVM

```
User clicks button
       ↓
Command in ViewModel triggers
       ↓
Service executes business logic
       ↓
ViewModel updates property
       ↓
INotifyPropertyChanged notifies UI
       ↓
Binding updates control
       ↓
User sees update
```

---

## 📝 PASO 1: Refactorizar Dashboard.xaml.cs

```csharp
using System.Windows;
using SistemaVentas.ViewModels;
using SistemaVentas.Services;

namespace SistemaVentas
{
    public partial class Dashboard : Window
    {
        public Dashboard(
            VentasService ventasService,
            SorteosService sorteosService,
            CajaService cajaService,
            ConfiguracionService configuracion,
            ReportesService reportes,
            NotificacionesService notificaciones)
        {
            InitializeComponent();

            // Crear ViewModel e inyectar servicios
            var viewModel = new DashboardViewModel(
                ventasService,
                sorteosService,
                cajaService,
                configuracion,
                reportes,
                notificaciones
            );

            // Establecer como DataContext
            this.DataContext = viewModel;

            // Inicializar datos
            this.Loaded += async (s, e) => await viewModel.InicializarAsync();
        }
    }
}
```

---

## 📋 PASO 2: Refactorizar Dashboard.xaml - Bindings

### Antes (Code-Behind):
```xaml
<Label x:Name="LblTotalVentas" Content="L 0.00" />
<Label x:Name="LblSaldoCaja" Content="L 0.00" />
<Button x:Name="BtnNuevaVenta" Click="BtnNuevaVenta_Click" />
```

### Ahora (MVVM):
```xaml
<Window
    xmlns:local="clr-namespace:SistemaVentas"
    x:Class="SistemaVentas.Dashboard"
    Title="Dashboard" Height="600" Width="1000">
    
    <Grid>
        <!-- Header -->
        <StackPanel Orientation="Horizontal" Margin="20">
            <TextBlock 
                Text="{Binding NombreNegocio}" 
                FontSize="24" FontWeight="Bold" />
            <TextBlock 
                Text="{Binding EstadoSistema}" 
                Margin="50,0,0,0" 
                Foreground="Green" />
        </StackPanel>

        <!-- Métricas -->
        <StackPanel Orientation="Vertical" Margin="20,80,20,20">
            
            <!-- Total Ventas -->
            <Border BorderBrush="LightGray" BorderThickness="1" Padding="15" Margin="0,0,0,10">
                <Grid>
                    <TextBlock Text="Total Ventas del Día" FontWeight="Bold" />
                    <TextBlock 
                        Text="{Binding TotalVentasHoy, StringFormat='L {0:N2}'}" 
                        FontSize="20" 
                        Foreground="Green" 
                        Margin="0,30,0,0" />
                </Grid>
            </Border>

            <!-- Saldo Caja -->
            <Border BorderBrush="LightGray" BorderThickness="1" Padding="15" Margin="0,0,0,10">
                <Grid>
                    <TextBlock Text="Saldo Caja" FontWeight="Bold" />
                    <TextBlock 
                        Text="{Binding SaldoCaja, StringFormat='L {0:N2}'}" 
                        FontSize="20" 
                        Foreground="Blue" 
                        Margin="0,30,0,0" />
                </Grid>
            </Border>

            <!-- Notificaciones -->
            <Border BorderBrush="LightGray" BorderThickness="1" Padding="15" Margin="0,0,0,10">
                <Grid>
                    <TextBlock 
                        Text="{Binding NotificacionesNoLeidas, StringFormat='Notificaciones no leídas: {0}'}" 
                        FontWeight="Bold" />
                </Grid>
            </Border>
        </StackPanel>

        <!-- Sorteos del Día -->
        <DataGrid 
            ItemsSource="{Binding SorteosDia}"
            Margin="20,350,20,80"
            AutoGenerateColumns="False">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Sorteo" Binding="{Binding Nombre}" Width="*" />
                <DataGridTextColumn Header="Hora" Binding="{Binding Hora}" Width="100" />
                <DataGridTextColumn Header="Estado" Binding="{Binding Estado}" Width="100" />
                <DataGridTextColumn Header="Resultado" Binding="{Binding Resultado}" Width="80" />
            </DataGrid.Columns>
        </DataGrid>

        <!-- Botones -->
        <StackPanel Orientation="Horizontal" VerticalAlignment="Bottom" Margin="20" Height="50">
            <Button 
                Content="Nueva Venta"
                Command="{Binding CrearVentaCommand}"
                Padding="20,10"
                Margin="0,0,10,0"
                IsEnabled="{Binding Cargando, Converter={StaticResource InverseBoolConverter}}" />
            
            <Button 
                Content="Actualizar"
                Command="{Binding ActualizarDatosCommand}"
                Padding="20,10"
                Margin="0,0,10,0"
                IsEnabled="{Binding Cargando, Converter={StaticResource InverseBoolConverter}}" />
            
            <Button 
                Content="Reporte Mensual"
                Command="{Binding GenerarReporteCommand}"
                Padding="20,10"
                IsEnabled="{Binding Cargando, Converter={StaticResource InverseBoolConverter}}" />
        </StackPanel>

        <!-- Loading Indicator -->
        <Border 
            Background="Black" 
            Opacity="0.3"
            Visibility="{Binding Cargando, Converter={StaticResource BoolToVisibilityConverter}}">
            <TextBlock 
                Text="Cargando..." 
                Foreground="White" 
                FontSize="18"
                VerticalAlignment="Center"
                HorizontalAlignment="Center" />
        </Border>
    </Grid>
</Window>
```

---

## 🔧 PASO 3: Agregar Converters en App.xaml

```xaml
<Application.Resources>
    <!-- Converters -->
    <local:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter" />
    <local:InverseBoolConverter x:Key="InverseBoolConverter" />
</Application.Resources>
```

---

## 📄 PASO 4: Crear Converters

**Archivo: SistemaVentas/Converters/BoolToVisibilityConverter.cs**
```csharp
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SistemaVentas.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible;
        }
    }
}
```

**Archivo: SistemaVentas/Converters/InverseBoolConverter.cs**
```csharp
using System;
using System.Globalization;
using System.Windows.Data;

namespace SistemaVentas.Converters
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}
```

---

## 🎯 PASO 5: Ventana de Nueva Venta

**Dashboard.xaml.cs - Comando de Nueva Venta:**
```csharp
private DashboardViewModel _viewModel;

protected override void OnContentRendered(EventArgs e)
{
    base.OnContentRendered(e);
    _viewModel = (DashboardViewModel)this.DataContext;
    
    // Interceptar comando
    var cmd = _viewModel.CrearVentaCommand as RelayCommand;
    // ... asociar con VentaWindow
}

private void AbrirVentaNueva()
{
    var window = new VentaWindow(
        App.ServiceProvider.GetService(typeof(VentasService)) as VentasService,
        App.ServiceProvider.GetService(typeof(SorteosService)) as SorteosService,
        usuarioId: 1 // Obtener del contexto
    );
    window.ShowDialog();
}
```

---

## ✅ Checklist de Implementación

- [ ] Crear BaseViewModel
- [ ] Crear DashboardViewModel
- [ ] Crear VentaViewModel
- [ ] Crear RelayCommand y AsyncRelayCommand
- [ ] Refactorizar Dashboard.xaml.cs
- [ ] Refactorizar Dashboard.xaml con bindings
- [ ] Crear Converters
- [ ] Crear VentaWindow
- [ ] Registrar ViewModels en App.xaml.cs
- [ ] Testear bindings

---

## 🧪 Ejemplo de Testing

```csharp
[TestClass]
public class DashboardViewModelTests
{
    [TestMethod]
    public async Task InicializarAsync_CargaDatos()
    {
        // Arrange
        var mockVentas = new Mock<VentasService>();
        var mockSorteos = new Mock<SorteosService>();
        var viewModel = new DashboardViewModel(mockVentas.Object, mockSorteos.Object, ...);

        // Act
        await viewModel.InicializarAsync();

        // Assert
        Assert.IsNotNull(viewModel.NombreNegocio);
        Assert.IsTrue(viewModel.SorteosDia.Count > 0);
    }
}
```

---

## 🚀 Beneficios de MVVM

| Aspecto | Beneficio |
|--------|-----------|
| **Testabilidad** | Código sin UI es fácil de testear |
| **Reutilización** | ViewModels pueden usarse en múltiples views |
| **Mantenibilidad** | Cambios en lógica sin afectar UI |
| **Separación** | Clear separation of concerns |
| **Binding** | Automático y bidireccional |
| **Data-driven** | UI reacciona a cambios de datos |

---

**Versión:** 2.0.0 - MVVM  
**Fecha:** 2026-07-16  
**Estado:** 🔄 En Implementación
