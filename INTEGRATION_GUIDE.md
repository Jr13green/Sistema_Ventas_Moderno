# 🎯 GUÍA MAESTRO DE INTEGRACIÓN - Sistema de Ventas Moderno

## 📋 Resumen Ejecutivo

Has completado la **FASE 1: Arquitectura Base** con éxito. Todos los servicios están creados y listos para integrar en tu Dashboard.

### ✅ Servicios Creados

| Servicio | Responsabilidad | Métodos Clave |
|----------|-----------------|---------------|
| **BaseDatosService** | Conexión y operaciones BD | `ObtenerConexion()`, `ExecuteScalarAsync()`, `ExecuteNonQueryAsync()` |
| **UsuariosService** | Autenticación y gestión de usuarios | `AutenticarAsync()`, `CrearUsuarioAsync()`, `ObtenerVendedoresActivosAsync()` |
| **VentasService** | Crear, anular y consultar ventas | `CrearVentaAsync()`, `AnularVentaAsync()`, `ObtenerVentasVendedorPorFechaAsync()` |
| **SorteosService** | Gestión de sorteos diarios | `CrearSorteosDelDiaAsync()`, `ActualizarEstadosSorteosAsync()`, `RegistrarResultadoAsync()` |
| **CajaService** | Movimientos de caja | `RegistrarAjusteAsync()`, `ObtenerSaldoCajaAsync()`, `ObtenerResumenCajaPorFechaAsync()` |
| **AuditoriaService** | Registro de operaciones | `RegistrarAsync()`, `ObtenerEventosAsync()`, `ObtenerEventosPorModuloAsync()` |
| **ConfiguracionService** | Gestión de configuración | `ObtenerTexto()`, `ObtenerDecimal()`, `GuardarAsync()` |
| **ReportesService** | Generación de reportes | `ObtenerResumenPeriodoAsync()`, `ObtenerTopVendedoresAsync()`, `ExportarVentasCSVAsync()` |
| **NotificacionesService** | Notificaciones del sistema | `CrearNotificacionAsync()`, `ObtenerConteoNoLeidasAsync()` |
| **RespaldoService** | Respaldos automáticos | `CrearRespaldoManualAsync()`, `CrearRespaldoAutomaticoAsync()` |

---

## 🔧 INTEGRACIÓN EN APP.XAML.CS

Primero, **actualiza tu App.xaml.cs** para registrar los servicios:

```csharp
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using SistemaVentas.Services;

namespace SistemaVentas
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configurar inyección de dependencias
            var services = new ServiceCollection();

            // Registrar servicios (Singleton = una única instancia)
            services.AddSingleton<BaseDatosService>();
            services.AddSingleton<AuditoriaService>();
            services.AddSingleton<ConfiguracionService>();
            services.AddSingleton<SorteosService>();
            services.AddSingleton<VentasService>();
            services.AddSingleton<UsuariosService>();
            services.AddSingleton<CajaService>();
            services.AddSingleton<ReportesService>();
            services.AddSingleton<NotificacionesService>();
            services.AddSingleton<RespaldoService>();

            // Registrar MainWindow
            services.AddSingleton<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();

            // Obtener e iniciar MainWindow
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
```

**Agregar estos Using en App.xaml.cs:**
```csharp
using Microsoft.Extensions.DependencyInjection;
using SistemaVentas.Services;
```

---

## 📱 INTEGRACIÓN EN DASHBOARD.XAML.CS

Así es cómo usar los servicios en tu Dashboard:

```csharp
using System.Windows;
using SistemaVentas.Services;

namespace SistemaVentas
{
    public partial class Dashboard : Window
    {
        // Inyectar servicios via constructor
        private readonly VentasService _ventasService;
        private readonly SorteosService _sorteosService;
        private readonly CajaService _cajaService;
        private readonly ConfiguracionService _configuracion;
        private readonly ReportesService _reportes;

        public Dashboard(
            VentasService ventasService,
            SorteosService sorteosService,
            CajaService cajaService,
            ConfiguracionService configuracion,
            ReportesService reportes)
        {
            InitializeComponent();

            _ventasService = ventasService;
            _sorteosService = sorteosService;
            _cajaService = cajaService;
            _configuracion = configuracion;
            _reportes = reportes;

            this.Loaded += Dashboard_Loaded;
        }

        private async void Dashboard_Loaded(object sender, RoutedEventArgs e)
        {
            // Inicializar datos
            await CargarDatosIniciales();
        }

        private async Task CargarDatosIniciales()
        {
            try
            {
                // Crear sorteos del día
                await _sorteosService.CrearSorteosDelDiaAsync();

                // Actualizar estados
                await _sorteosService.ActualizarEstadosSorteosAsync();

                // Obtener datos del día
                var sorteosDia = await _sorteosService.ObtenerSorteosDiarioAsync(DateTime.Today);
                var totalVentas = await _ventasService.ObtenerTotalVentasActivasPorFechaAsync(DateTime.Today);
                var saldoCaja = await _cajaService.ObtenerSaldoCajaAsync(DateTime.Today);

                // Actualizar UI
                LblTotalVentas.Content = $\"L {totalVentas:N2}\";
                LblSaldoCaja.Content = $\"L {saldoCaja:N2}\";
            }
            catch (Exception ex)
            {
                MessageBox.Show($\"Error: {ex.Message}\", \"Error\", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Ejemplo: Crear una venta
        private async void BtnNuevaVenta_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var jugadas = new List<(long sorteoDiarioId, string numero, decimal monto)>
                {
                    (1, \"05\", 50),
                    (2, \"12\", 100)
                };

                var (exito, mensaje, ventaId) = await _ventasService.CrearVentaAsync(
                    usuarioId: 1,
                    jugadas: jugadas
                );

                if (exito)
                    MessageBox.Show($\"Venta creada: {ventaId}\");
                else
                    MessageBox.Show($\"Error: {mensaje}\");
            }
            catch (Exception ex)
            {
                MessageBox.Show($\"Error: {ex.Message}\");
            }
        }

        // Ejemplo: Obtener reporte
        private async void BtnReporte_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var (ventas, premios, ganancia, transacciones) = 
                    await _reportes.ObtenerResumenPeriodoAsync(
                        DateTime.Today.AddDays(-30),
                        DateTime.Today
                    );

                MessageBox.Show($\"Ventas: L {ventas:N2}\\nPremios: L {premios:N2}\\nGanancia: L {ganancia:N2}\");
            }
            catch (Exception ex)
            {
                MessageBox.Show($\"Error: {ex.Message}\");
            }
        }
    }
}
```

---

## 🔄 FLUJO TÍPICO DE UNA VENTA

```
1. Usuario inicia sesión
   ↓
2. Sistema crea sorteos del día
   ↓
3. Usuario selecciona sorteo + número + monto
   ↓
4. VentasService.CrearVentaAsync() → Inserta en BD
   ↓
5. AuditoriaService registra la operación
   ↓
6. NotificacionesService notifica (opcional)
   ↓
7. Actualizar UI con nuevos datos
```

---

## 📊 EJEMPLOS DE USO POR SERVICIO

### VentasService
```csharp
// Crear venta
var jugadas = new List<(long sorteoDiarioId, string numero, decimal monto)>
{
    (1, \"05\", 50),
    (2, \"12\", 100)
};
var (exito, msg, id) = await _ventasService.CrearVentaAsync(1, jugadas);

// Obtener ventas del día
var ventasHoy = await _ventasService.ObtenerVentasVendedorPorFechaAsync(1, DateTime.Today);

// Anular venta
await _ventasService.AnularVentaAsync(1, usuarioId: 1);
```

### SorteosService
```csharp
// Crear sorteos del día
await _sorteosService.CrearSorteosDelDiaAsync();

// Actualizar estados automáticamente
await _sorteosService.ActualizarEstadosSorteosAsync();

// Registrar resultado
await _sorteosService.RegistrarResultadoAsync(1, \"25\", usuarioId: 1);

// Obtener próximo sorteo
var proximo = await _sorteosService.ObtenerProximoSorteoAsync();
```

### ConfiguracionService
```csharp
// Inicializar caché (una sola vez al inicio)
await _configuracion.InicializarCacheAsync();

// Obtener valores
string negocio = _configuracion.ObtenerNombreNegocio();
decimal multiplicador = _configuracion.ObtenerMultiplicadorPremio();

// Guardar configuración
await _configuracion.GuardarAsync(\"MiClave\", \"MiValor\");
```

### ReportesService
```csharp
// Reporte completo
var (ventas, premios, ganancia, txns) = 
    await _reportes.ObtenerResumenPeriodoAsync(inicio, fin);

// Top vendedores
var topVendedores = await _reportes.ObtenerTopVendedoresAsync(inicio, fin, 5);

// Ganadores
var ganadores = await _reportes.ObtenerGanadoresAsync(inicio, fin);

// Exportar CSV
string csv = await _reportes.ExportarVentasCSVAsync(inicio, fin);
```

---

## ⚙️ CAMBIOS NECESARIOS EN LA BD

Algunos servicios requieren tablas adicionales. Actualiza tu `BaseDatosService.InicializarBaseDatosAsync()`:

```sql
-- Agregar estas tablas en el SQL de inicialización:

CREATE TABLE IF NOT EXISTS AjustesCaja (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Tipo TEXT NOT NULL,
    Monto REAL NOT NULL,
    Motivo TEXT,
    UsuarioId INTEGER,
    FechaCreacion TEXT NOT NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);

CREATE TABLE IF NOT EXISTS Notificaciones (
    Clave TEXT PRIMARY KEY,
    Titulo TEXT NOT NULL,
    Detalle TEXT NOT NULL,
    Prioridad TEXT,
    Modulo TEXT,
    Activa INTEGER DEFAULT 1,
    Leida INTEGER DEFAULT 0,
    FechaActualizacion TEXT
);
```

---

## 🚨 IMPORTANTE: Verificación de Nombres

En tu código actual, verifica que los **nombres de las propiedades de modelos** coincidan con lo esperado. Si usas nombres diferentes, actualiza las queries SQL.

Por ejemplo, si tu modelo `Venta` usa `Total` en lugar de `Monto`, asegúrate que las queries reflejen esto.

---

## 📌 PRÓXIMOS PASOS

### Fase 2: Refactor UI
- [ ] Separar lógica de negocio del code-behind
- [ ] Crear ViewModels
- [ ] Implementar MVVM pattern
- [ ] Binding a datos desde servicios

### Fase 3: Optimización
- [ ] Agregar caché distribuido (Redis)
- [ ] Implementar paginación
- [ ] Agregar filtros avanzados
- [ ] Optimizar queries SQL

### Fase 4: Testing
- [ ] Crear tests unitarios
- [ ] Tests de integración
- [ ] Cobertura de código >80%

---

## 🎓 ARQUITECTURA FINAL

```
┌─────────────────────────────────┐
│      UI Layer (XAML)            │
├─────────────────────────────────┤
│    ViewModel Layer (Logic)      │
├─────────────────────────────────┤
│    Services Layer (Business)    │ ← **TÚ ESTÁS AQUÍ**
├─────────────────────────────────┤
│  Repository Layer (Data Access) │ ← Próximo paso
├─────────────────────────────────┤
│    Database Layer (SQLite)      │
└─────────────────────────────────┘
```

---

## 📞 Soporte

- **Rama:** `feature/services`
- **Commits:** Cada servicio tiene un commit descriptivo
- **Documentación:** Cada método tiene comentarios XML

---

**Versión:** 1.0.0  
**Fecha:** 2026-07-16  
**Estado:** ✅ COMPLETO
