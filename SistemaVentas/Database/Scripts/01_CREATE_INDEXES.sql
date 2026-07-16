-- ============================================================
-- FASE 4: Índices de optimización para Sistema de Ventas
-- Script: 01_CREATE_INDEXES.sql
-- Descripción: Índices en columnas clave para mejorar performance
-- ============================================================

-- ============================================================
-- Tabla: Vendedores / Usuarios
-- ============================================================
-- Índice para búsqueda por nombre (filtros y reportes)
CREATE INDEX IF NOT EXISTS IX_Usuarios_Nombre
    ON Usuarios (Nombre);

-- Índice para búsqueda por número de teléfono (login)
CREATE INDEX IF NOT EXISTS IX_Usuarios_Numero
    ON Usuarios (Numero);

-- Índice compuesto para búsquedas activas por nombre
CREATE INDEX IF NOT EXISTS IX_Usuarios_Activo_Nombre
    ON Usuarios (Activo, Nombre);

-- ============================================================
-- Tabla: Sorteos
-- ============================================================
-- Índice para filtrar sorteos por fecha (consultas diarias)
CREATE INDEX IF NOT EXISTS IX_SorteosDiarios_Fecha
    ON SorteosDiarios (Fecha DESC);

-- Índice para filtrar sorteos por estado
CREATE INDEX IF NOT EXISTS IX_SorteosDiarios_Estado
    ON SorteosDiarios (Estado);

-- Índice compuesto: fecha + estado (el más frecuente)
CREATE INDEX IF NOT EXISTS IX_SorteosDiarios_Fecha_Estado
    ON SorteosDiarios (Fecha DESC, Estado);

-- ============================================================
-- Tabla: Ventas
-- ============================================================
-- Índice para consultas por fecha
CREATE INDEX IF NOT EXISTS IX_Ventas_Fecha
    ON Ventas (Fecha DESC);

-- Índice para consultas por usuario
CREATE INDEX IF NOT EXISTS IX_Ventas_UsuarioId
    ON Ventas (UsuarioId);

-- Índice compuesto: usuario + fecha (reportes de vendedor)
CREATE INDEX IF NOT EXISTS IX_Ventas_UsuarioId_Fecha
    ON Ventas (UsuarioId, Fecha DESC);

-- Índice para filtrar por estado
CREATE INDEX IF NOT EXISTS IX_Ventas_Estado
    ON Ventas (Estado);

-- ============================================================
-- Tabla: DetallesVenta (Jugadas)
-- ============================================================
-- Índice para búsqueda por VentaId
CREATE INDEX IF NOT EXISTS IX_DetallesVenta_VentaId
    ON DetallesVenta (VentaId);

-- Índice para búsqueda por sorteo diario
CREATE INDEX IF NOT EXISTS IX_DetallesVenta_SorteoDiarioId
    ON DetallesVenta (SorteoDiarioId);

-- Índice para búsqueda de números jugados
CREATE INDEX IF NOT EXISTS IX_DetallesVenta_Numero
    ON DetallesVenta (Numero);

-- ============================================================
-- Tabla: MovimientosCaja
-- ============================================================
-- Índice para consultas por fecha
CREATE INDEX IF NOT EXISTS IX_MovimientosCaja_Fecha
    ON MovimientosCaja (Fecha DESC);

-- Índice para consultas por tipo
CREATE INDEX IF NOT EXISTS IX_MovimientosCaja_Tipo
    ON MovimientosCaja (Tipo);

-- Índice compuesto: fecha + tipo (reportes de caja)
CREATE INDEX IF NOT EXISTS IX_MovimientosCaja_Fecha_Tipo
    ON MovimientosCaja (Fecha DESC, Tipo);

-- ============================================================
-- Tabla: Auditoria
-- ============================================================
-- Índice para consultas por fecha
CREATE INDEX IF NOT EXISTS IX_Auditoria_Fecha
    ON Auditoria (Fecha DESC);

-- Índice para filtrar por módulo
CREATE INDEX IF NOT EXISTS IX_Auditoria_Modulo
    ON Auditoria (Modulo);

-- Índice para filtrar por usuario
CREATE INDEX IF NOT EXISTS IX_Auditoria_UsuarioId
    ON Auditoria (UsuarioId);

-- ============================================================
-- Verificar índices creados
-- ============================================================
-- SELECT name, tbl_name FROM sqlite_master WHERE type = 'index' ORDER BY tbl_name, name;

PRAGMA optimize;
