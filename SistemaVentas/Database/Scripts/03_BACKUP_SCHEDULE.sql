-- ============================================================
-- FASE 4: Configuración de respaldo automático
-- Script: 03_BACKUP_SCHEDULE.sql
-- Descripción: Estrategia y configuración de backups para SQLite
-- ============================================================

-- ============================================================
-- Configuración WAL (Write-Ahead Logging) para mejor performance
-- y consistencia en backups en caliente
-- ============================================================
PRAGMA journal_mode = WAL;
PRAGMA synchronous = NORMAL;
PRAGMA wal_autocheckpoint = 1000;

-- ============================================================
-- Tabla de registro de backups realizados
-- ============================================================
CREATE TABLE IF NOT EXISTS BackupLog (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Fecha           TEXT    NOT NULL DEFAULT (datetime('now','localtime')),
    TipoBackup      TEXT    NOT NULL,   -- 'Diario', 'Semanal', 'Mensual', 'Manual'
    RutaArchivo     TEXT    NOT NULL,
    TamañoBytes     INTEGER,
    DuracionMs      INTEGER,
    Exitoso         INTEGER NOT NULL DEFAULT 1,  -- 1=true, 0=false
    MensajeError    TEXT
);

-- Índice para consultas de historial
CREATE INDEX IF NOT EXISTS IX_BackupLog_Fecha
    ON BackupLog (Fecha DESC);

-- ============================================================
-- Tabla de configuración de backups automáticos
-- ============================================================
CREATE TABLE IF NOT EXISTS ConfiguracionBackup (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    TipoBackup      TEXT    NOT NULL UNIQUE,
    Habilitado      INTEGER NOT NULL DEFAULT 1,
    HoraEjecucion   TEXT    NOT NULL DEFAULT '02:00',  -- HH:mm formato 24h
    DiaSemana       INTEGER,                            -- 0=Dom..6=Sab (para semanal)
    DiaMes          INTEGER,                            -- 1-28 (para mensual)
    RetenciónDias   INTEGER NOT NULL DEFAULT 30,
    RutaDestino     TEXT    NOT NULL DEFAULT 'Backups/',
    ComprimirZip    INTEGER NOT NULL DEFAULT 1,
    FechaModificado TEXT    NOT NULL DEFAULT (datetime('now','localtime'))
);

-- Insertar configuración por defecto
INSERT OR IGNORE INTO ConfiguracionBackup (TipoBackup, Habilitado, HoraEjecucion, RetenciónDias, RutaDestino)
VALUES
    ('Diario',   1, '02:00', 7,  'Backups/Diarios/'),
    ('Semanal',  1, '03:00', 30, 'Backups/Semanales/'),
    ('Mensual',  1, '04:00', 365,'Backups/Mensuales/');

-- Actualizar configuración semanal (domingos = 0)
UPDATE ConfiguracionBackup SET DiaSemana = 0 WHERE TipoBackup = 'Semanal';

-- Actualizar configuración mensual (día 1 de cada mes)
UPDATE ConfiguracionBackup SET DiaMes = 1 WHERE TipoBackup = 'Mensual';

-- ============================================================
-- Vista: Estado de backups recientes
-- ============================================================
DROP VIEW IF EXISTS V_EstadoBackups;
CREATE VIEW V_EstadoBackups AS
SELECT
    cb.TipoBackup,
    cb.Habilitado,
    cb.HoraEjecucion,
    cb.RetenciónDias,
    cb.RutaDestino,
    MAX(bl.Fecha)       AS UltimoBackup,
    bl.Exitoso          AS UltimoExitoso,
    bl.TamañoBytes      AS UltimoTamaño
FROM ConfiguracionBackup cb
LEFT JOIN BackupLog bl ON bl.TipoBackup = cb.TipoBackup
GROUP BY cb.TipoBackup, cb.Habilitado, cb.HoraEjecucion, cb.RetenciónDias, cb.RutaDestino;

-- ============================================================
-- Notas de implementación en C#:
-- El servicio RespaldoService.cs ejecuta el backup usando
-- "VACUUM INTO 'ruta_backup.db'" para SQLite.
-- El programador de tareas de Windows (Task Scheduler) o
-- un BackgroundService de .NET puede disparar el backup.
-- ============================================================
