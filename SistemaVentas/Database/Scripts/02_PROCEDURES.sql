-- ============================================================
-- FASE 4: Queries optimizadas y vistas para Sistema de Ventas
-- Script: 02_PROCEDURES.sql
-- Descripción: Vistas y queries optimizadas para operaciones frecuentes
-- Nota: SQLite no soporta stored procedures; se usan vistas y queries
-- ============================================================

-- ============================================================
-- Vista: Resumen de ventas del día
-- ============================================================
DROP VIEW IF EXISTS V_ResumenVentasDia;
CREATE VIEW V_ResumenVentasDia AS
SELECT
    DATE(v.Fecha)           AS Dia,
    COUNT(DISTINCT v.Id)    AS TotalVentas,
    SUM(dv.Monto)           AS MontoTotal,
    COUNT(DISTINCT v.UsuarioId) AS TotalVendedores
FROM Ventas v
INNER JOIN DetallesVenta dv ON dv.VentaId = v.Id
WHERE v.Estado = 'Activa'
GROUP BY DATE(v.Fecha);

-- ============================================================
-- Vista: Ventas con datos de vendedor
-- ============================================================
DROP VIEW IF EXISTS V_VentasConVendedor;
CREATE VIEW V_VentasConVendedor AS
SELECT
    v.Id            AS VentaId,
    v.Fecha,
    v.Estado,
    u.Nombre        AS Vendedor,
    u.Numero        AS TelefonoVendedor,
    SUM(dv.Monto)   AS MontoTotal,
    COUNT(dv.Id)    AS TotalJugadas
FROM Ventas v
INNER JOIN Usuarios u  ON u.Id  = v.UsuarioId
INNER JOIN DetallesVenta dv ON dv.VentaId = v.Id
GROUP BY v.Id, v.Fecha, v.Estado, u.Nombre, u.Numero;

-- ============================================================
-- Vista: Top vendedores por período
-- ============================================================
DROP VIEW IF EXISTS V_TopVendedores;
CREATE VIEW V_TopVendedores AS
SELECT
    u.Id            AS VendedorId,
    u.Nombre,
    u.Numero,
    COUNT(DISTINCT v.Id)    AS TotalVentas,
    SUM(dv.Monto)           AS MontoTotal,
    DATE(v.Fecha)           AS Dia
FROM Usuarios u
INNER JOIN Ventas v   ON v.UsuarioId = u.Id AND v.Estado = 'Activa'
INNER JOIN DetallesVenta dv ON dv.VentaId = v.Id
GROUP BY u.Id, u.Nombre, u.Numero, DATE(v.Fecha)
ORDER BY MontoTotal DESC;

-- ============================================================
-- Vista: Sorteos del día con estadísticas
-- ============================================================
DROP VIEW IF EXISTS V_SorteosDiaStats;
CREATE VIEW V_SorteosDiaStats AS
SELECT
    sd.Id,
    sd.Nombre,
    sd.Fecha,
    sd.Estado,
    sd.HoraLimite,
    COUNT(dv.Id)    AS TotalJugadas,
    SUM(dv.Monto)   AS MontoRecaudado,
    COUNT(DISTINCT v.UsuarioId) AS TotalVendedores
FROM SorteosDiarios sd
LEFT JOIN DetallesVenta dv ON dv.SorteoDiarioId = sd.Id
LEFT JOIN Ventas v         ON v.Id = dv.VentaId AND v.Estado = 'Activa'
GROUP BY sd.Id, sd.Nombre, sd.Fecha, sd.Estado, sd.HoraLimite;

-- ============================================================
-- Vista: Saldo actual de caja por día
-- ============================================================
DROP VIEW IF EXISTS V_SaldoCajaDia;
CREATE VIEW V_SaldoCajaDia AS
SELECT
    DATE(Fecha)    AS Dia,
    SUM(CASE WHEN Tipo = 'Ingreso'  THEN Monto ELSE 0 END) AS TotalIngresos,
    SUM(CASE WHEN Tipo = 'Egreso'   THEN Monto ELSE 0 END) AS TotalEgresos,
    SUM(CASE WHEN Tipo = 'Ingreso'  THEN Monto ELSE -Monto END) AS SaldoFinal
FROM MovimientosCaja
GROUP BY DATE(Fecha);

-- ============================================================
-- Vista: Reporte de ganadores por sorteo
-- ============================================================
DROP VIEW IF EXISTS V_Ganadores;
CREATE VIEW V_Ganadores AS
SELECT
    sd.Id   AS SorteoDiarioId,
    sd.Nombre AS NombreSorteo,
    sd.Fecha,
    sd.NumeroGanador,
    dv.Numero AS NumeroJugado,
    dv.Monto  AS MontoApostado,
    u.Nombre  AS Vendedor,
    u.Numero  AS TelefonoVendedor
FROM SorteosDiarios sd
INNER JOIN DetallesVenta dv ON dv.SorteoDiarioId = sd.Id
                            AND dv.Numero = sd.NumeroGanador
INNER JOIN Ventas v         ON v.Id = dv.VentaId AND v.Estado = 'Activa'
INNER JOIN Usuarios u       ON u.Id = v.UsuarioId
WHERE sd.NumeroGanador IS NOT NULL;

-- ============================================================
-- Query optimizada: Obtener ventas por período (usar con parámetros)
-- Uso: SELECT * FROM V_VentasConVendedor WHERE Fecha BETWEEN :inicio AND :fin
-- ============================================================

-- ============================================================
-- Query optimizada: Total ventas por vendedor en rango de fechas
-- ============================================================
-- SELECT VendedorId, Nombre, SUM(MontoTotal) AS Total
-- FROM V_TopVendedores
-- WHERE Dia BETWEEN :fechaInicio AND :fechaFin
-- GROUP BY VendedorId, Nombre
-- ORDER BY Total DESC
-- LIMIT 10;

-- ============================================================
-- Trigger: Actualizar estado de sorteo automáticamente
-- ============================================================
DROP TRIGGER IF EXISTS TR_SorteosDiarios_EstadoAutomatico;
CREATE TRIGGER TR_SorteosDiarios_EstadoAutomatico
    AFTER UPDATE OF Estado ON SorteosDiarios
    WHEN NEW.Estado = 'Cerrado' AND NEW.NumeroGanador IS NULL
BEGIN
    -- Marcar jugadas como procesadas cuando el sorteo cierra sin ganador
    UPDATE DetallesVenta
    SET Procesado = 1
    WHERE SorteoDiarioId = NEW.Id;
END;

PRAGMA integrity_check;
