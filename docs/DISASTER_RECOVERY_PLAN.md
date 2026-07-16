# 🛡️ Plan de Disaster Recovery

## Sistema de Ventas Diaria Familiar — FASE 4

---

## 📊 Objetivos de Recuperación

| Métrica | Objetivo | Descripción |
|---------|----------|-------------|
| **RTO** (Recovery Time Objective) | 4 horas | Tiempo máximo para restaurar el servicio |
| **RPO** (Recovery Point Objective) | 24 horas | Máxima pérdida de datos tolerable |
| **Disponibilidad objetivo** | 99.9% | ≈ 8.7 horas de downtime/año |

---

## 🗄️ Estrategia de Backup

### Backups automáticos (SQLite)
```
Diario   → 02:00 AM → /backups/Diarios/   → retención 7 días
Semanal  → 03:00 AM domingo → /backups/Semanales/ → retención 30 días
Mensual  → 04:00 AM día 1 → /backups/Mensuales/ → retención 365 días
```

### Comando de backup manual
```bash
sqlite3 /data/SistemaVentas.db \
  "VACUUM INTO '/backups/manual/backup_$(date +%Y%m%d_%H%M%S).db';"
```

### Copias offsite
- Backups semanales → almacenamiento externo (USB/NAS/nube)
- Verificar integridad mensualmente con `PRAGMA integrity_check;`

---

## 🔴 Escenarios de Desastre

### Escenario 1: Corrupción de base de datos

**Detección:** Health check retorna `503`, error en `BaseDatos` component.

**Pasos:**
1. Detener el servicio inmediatamente
2. Hacer copia del archivo corrupto para análisis
3. Identificar el backup más reciente válido
4. Restaurar backup:
   ```bash
   cp /backups/Diarios/ultimo_backup.db /data/SistemaVentas.db
   ```
5. Verificar integridad:
   ```bash
   sqlite3 /data/SistemaVentas.db "PRAGMA integrity_check;"
   ```
6. Reiniciar el servicio
7. Documentar el incidente y pérdida de datos (si aplica)

**RTO estimado:** 30-60 minutos

---

### Escenario 2: Fallo del servidor

**Detección:** Servicio inaccesible, health check sin respuesta.

**Pasos:**
1. Verificar estado del servidor (ping, acceso SSH/RDP)
2. Si el servidor no responde, activar servidor de contingencia
3. Copiar backups al servidor de contingencia
4. Desplegar aplicación:
   ```powershell
   # Copiar publish y restaurar BD
   Copy-Item \\backup-server\SistemaVentasAPI -Destination C:\apps\ -Recurse
   Copy-Item \\backup-server\backup.db -Destination C:\Data\SistemaVentas.db
   ```
5. Iniciar servicio
6. Actualizar DNS/hosts para apuntar al nuevo servidor

**RTO estimado:** 2-4 horas

---

### Escenario 3: Brecha de seguridad

**Detección:** Accesos no autorizados en logs de auditoría, comportamiento anómalo.

**Pasos inmediatos:**
1. Revocar todos los tokens JWT activos (cambiar `JwtSecretKey`)
2. Cambiar todas las contraseñas del sistema
3. Analizar logs de acceso para determinar alcance
4. Notificar a usuarios afectados
5. Ejecutar backup de estado actual para evidencia forense
6. Re-encriptar datos sensibles con nueva clave si es necesario
7. Reportar el incidente según regulaciones aplicables

**RTO estimado:** Variable (depende del alcance)

---

### Escenario 4: Error de deployment

**Detección:** Health check falla después de un deploy, errores en logs.

**Pasos (Docker):**
```bash
# Rollback a versión anterior
docker-compose stop api
docker tag sistemaventas-api:current sistemaventas-api:failed
docker pull sistemaventas-api:previous
docker-compose up -d api

# Verificar salud
curl http://localhost:5000/health
```

**RTO estimado:** 15-30 minutos

---

## 📞 Árbol de Comunicación

```
Incidente detectado
       ↓
Operaciones (30 min)
       ↓ (si no resuelto)
Desarrollo (2 horas)
       ↓ (si crítico)
Gerencia + Arquitecto (inmediato)
       ↓ (si afecta datos de clientes)
Equipo legal + Usuarios afectados
```

---

## 🧪 Pruebas del Plan DR

**Frecuencia:** Trimestralmente

**Procedimiento:**
1. Simular fallo de BD (sin afectar producción)
2. Restaurar desde backup en entorno de prueba
3. Medir RTO real vs objetivo
4. Documentar lecciones aprendidas
5. Actualizar este plan si es necesario

---

**Versión:** 4.0.0  
**Última actualización:** 2026-07-16  
**Próxima revisión:** 2026-10-16
