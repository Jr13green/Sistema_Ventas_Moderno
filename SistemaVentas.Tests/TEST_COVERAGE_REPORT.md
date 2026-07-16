# Reporte de Cobertura (FASE 3)

## Objetivo
Cobertura general >80%.

## Alcance cubierto
- ViewModels (Dashboard, Venta, Reporte, Caja, Usuarios)
- Services (Ventas, Sorteos, Caja, Usuarios, Reportes)
- Commands (RelayCommand, AsyncRelayCommand)
- Converters (BoolToVisibilityConverter, DecimalToStringConverter)
- Integration tests (Dashboard, Venta, Caja)

## Generar reporte local

```bash
dotnet test /home/runner/work/Sistema_Ventas_Moderno/Sistema_Ventas_Moderno/SistemaVentas.Tests/SistemaVentas.Tests.csproj /p:EnableWindowsTargeting=true --collect:"XPlat Code Coverage"
```

El archivo de cobertura (`coverage.cobertura.xml`) se genera en `TestResults/**`.
