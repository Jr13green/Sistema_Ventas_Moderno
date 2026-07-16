# Testing en SistemaVentas

## Requisitos
- .NET 10 SDK

## Ejecutar tests

```bash
dotnet test /home/runner/work/Sistema_Ventas_Moderno/Sistema_Ventas_Moderno/SistemaVentas.Tests/SistemaVentas.Tests.csproj /p:EnableWindowsTargeting=true
```

## Ejecutar con cobertura

```bash
dotnet test /home/runner/work/Sistema_Ventas_Moderno/Sistema_Ventas_Moderno/SistemaVentas.Tests/SistemaVentas.Tests.csproj /p:EnableWindowsTargeting=true --collect:"XPlat Code Coverage"
```

## Estructura
- `ViewModelTests/`: pruebas unitarias de ViewModels.
- `ServiceTests/`: pruebas unitarias de servicios.
- `CommandTests/`: pruebas de comandos MVVM.
- `ConverterTests/`: pruebas de converters.
- `IntegrationTests/`: flujos principales.
- `Fixtures/`: mocks y datos reutilizables.
