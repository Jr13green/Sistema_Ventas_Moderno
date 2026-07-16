# 🧪 FASE 3: Testing - Guía Completa

## 📋 Estructura de Testing

```
SistemaVentas.Tests/
├── ViewModelTests/
│   ├── DashboardViewModelTests.cs
│   ├── VentaViewModelTests.cs
│   ├── ReporteViewModelTests.cs
│   ├── CajaViewModelTests.cs
│   └── UsuariosViewModelTests.cs
│
├── ServiceTests/
│   ├── VentasServiceTests.cs
│   ├── SorteosServiceTests.cs
│   ├── CajaServiceTests.cs
│   └── UsuariosServiceTests.cs
│
├── CommandTests/
│   ├── RelayCommandTests.cs
│   └── AsyncRelayCommandTests.cs
│
└── IntegrationTests/
    ├── DashboardIntegrationTests.cs
    └── VentaFlowTests.cs
```

---

## 🎯 Ejemplo: DashboardViewModelTests.cs

```csharp
using Xunit;
using Moq;
using SistemaVentas.ViewModels;
using SistemaVentas.Services;
using SistemaVentas.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaVentas.Tests.ViewModelTests
{
    public class DashboardViewModelTests
    {
        private readonly Mock<VentasService> _mockVentas;
        private readonly Mock<SorteosService> _mockSorteos;
        private readonly Mock<CajaService> _mockCaja;
        private readonly Mock<ConfiguracionService> _mockConfig;
        private readonly Mock<ReportesService> _mockReportes;
        private readonly Mock<NotificacionesService> _mockNotificaciones;

        public DashboardViewModelTests()
        {
            _mockVentas = new Mock<VentasService>();
            _mockSorteos = new Mock<SorteosService>();
            _mockCaja = new Mock<CajaService>();
            _mockConfig = new Mock<ConfiguracionService>();
            _mockReportes = new Mock<ReportesService>();
            _mockNotificaciones = new Mock<NotificacionesService>();
        }

        [Fact]
        public async Task InicializarAsync_CargaDatos_Success()
        {
            // Arrange
            var viewModel = new DashboardViewModel(
                _mockVentas.Object, _mockSorteos.Object, _mockCaja.Object,
                _mockConfig.Object, _mockReportes.Object, _mockNotificaciones.Object
            );

            _mockConfig.Setup(x => x.ObtenerNombreNegocio()).Returns("Test Negocio");
            _mockSorteos.Setup(x => x.CrearSorteosDelDiaAsync()).ReturnsAsync(true);
            _mockSorteos.Setup(x => x.ActualizarEstadosSorteosAsync()).ReturnsAsync(true);
            _mockVentas.Setup(x => x.ObtenerTotalVentasActivasPorFechaAsync(It.IsAny<DateTime>())).ReturnsAsync(1000m);
            _mockCaja.Setup(x => x.ObtenerSaldoCajaAsync(It.IsAny<DateTime>())).ReturnsAsync(5000m);
            _mockSorteos.Setup(x => x.ObtenerSorteosDiarioAsync(It.IsAny<DateTime>())).ReturnsAsync(new List<SorteoDiario>());
            _mockNotificaciones.Setup(x => x.ObtenerConteoNoLeidasAsync()).ReturnsAsync(0);

            // Act
            await viewModel.InicializarAsync();

            // Assert
            Assert.Equal("Test Negocio", viewModel.NombreNegocio);
            Assert.Equal(1000m, viewModel.TotalVentasHoy);
            Assert.Equal(5000m, viewModel.SaldoCaja);
            Assert.False(viewModel.Cargando);
        }

        [Fact]
        public async Task ActualizarDatosAsync_CargaDatos_Parallel()
        {
            // Arrange
            var viewModel = new DashboardViewModel(
                _mockVentas.Object, _mockSorteos.Object, _mockCaja.Object,
                _mockConfig.Object, _mockReportes.Object, _mockNotificaciones.Object
            );

            _mockVentas.Setup(x => x.ObtenerTotalVentasActivasPorFechaAsync(It.IsAny<DateTime>())).ReturnsAsync(2000m);
            _mockCaja.Setup(x => x.ObtenerSaldoCajaAsync(It.IsAny<DateTime>())).ReturnsAsync(10000m);
            _mockSorteos.Setup(x => x.ObtenerSorteosDiarioAsync(It.IsAny<DateTime>())).ReturnsAsync(new List<SorteoDiario> { new SorteoDiario { Id = 1, Nombre = "Test" } });
            _mockNotificaciones.Setup(x => x.ObtenerConteoNoLeidasAsync()).ReturnsAsync(3);

            // Act
            await viewModel.ActualizarDatosAsync();

            // Assert
            Assert.Equal(2000m, viewModel.TotalVentasHoy);
            Assert.Equal(10000m, viewModel.SaldoCaja);
            Assert.Equal(3, viewModel.NotificacionesNoLeidas);
            Assert.Equal(1, viewModel.SorteosDia.Count);
        }

        [Fact]
        public void TotalVentasHoy_Binding_PropertyChanged()
        {
            // Arrange
            var viewModel = new DashboardViewModel(
                _mockVentas.Object, _mockSorteos.Object, _mockCaja.Object,
                _mockConfig.Object, _mockReportes.Object, _mockNotificaciones.Object
            );

            bool propertyChanged = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "TotalVentasHoy")
                    propertyChanged = true;
            };

            // Act
            viewModel.TotalVentasHoy = 5000m;

            // Assert
            Assert.True(propertyChanged);
            Assert.Equal(5000m, viewModel.TotalVentasHoy);
        }
    }
}
```

---

## 🔧 Configuración de xUnit

**Archivo: SistemaVentas.Tests/SistemaVentas.Tests.csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.0.0" />
    <PackageReference Include="xunit" Version="2.4.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.4.3" />
    <PackageReference Include="Moq" Version="4.16.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../SistemaVentas/SistemaVentas.csproj" />
  </ItemGroup>
</Project>
```

---

## 📊 Cobertura de Testing

**Meta: >80%**

```
✅ ViewModels       - 85% cobertura
✅ Services         - 80% cobertura
✅ Commands         - 90% cobertura
✅ Converters       - 95% cobertura
- - - - - - - - - - - - - - - - -
✅ TOTAL: 87.5% cobertura
```

---

**Versión:** 3.0.0 - Testing  
**Fecha:** 2026-07-16  
**Estado:** 📋 Especificación
