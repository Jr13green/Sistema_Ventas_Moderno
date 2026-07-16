# 🔧 Runbook de Troubleshooting

## Sistema de Ventas Diaria Familiar — FASE 4

---

## 🚨 Problemas Comunes y Soluciones

### 1. API no inicia

**Síntoma:** El servicio `SistemaVentasAPI` no inicia o falla al arrancar.

**Diagnóstico:**
```powershell
# Windows Service
Get-EventLog -LogName Application -Source "SistemaVentasAPI" -Newest 20

# Docker
docker-compose logs api --tail=100
```

**Causas y soluciones:**
| Causa | Solución |
|-------|----------|
| Puerto 5000 ocupado | `netstat -ano \| findstr :5000` → Terminar proceso |
| JWT secret no configurado | Verificar variable `Security__JwtSecretKey` |
| BD corrupta | Restaurar desde backup (ver sección BD) |
| RAM insuficiente | Aumentar RAM o reducir `Cache:MaxSizeItems` |

---

### 2. Error de base de datos

**Síntoma:** `Error al verificar base de datos` en `/health`.

**Diagnóstico:**
```bash
# Verificar integridad de SQLite
sqlite3 /data/SistemaVentas.db "PRAGMA integrity_check;"

# Ver tamaño del archivo
ls -lh /data/SistemaVentas.db
```

**Soluciones:**
```bash
# Reparar WAL corrupto
sqlite3 /data/SistemaVentas.db "PRAGMA wal_checkpoint(TRUNCATE);"

# Si está corrupta, restaurar desde backup
cp /backups/Diarios/ultimo_backup.db /data/SistemaVentas.db

# Reindexar
sqlite3 /data/SistemaVentas.db "REINDEX;"
```

---

### 3. Rate limiting agresivo

**Síntoma:** Clientes reciben `429 Too Many Requests`.

**Ajustar configuración:**
```json
// appsettings.Production.json
{
  "Api": {
    "RateLimitRequestsPerMinute": 120
  }
}
```

**Reiniciar servicio:**
```bash
docker-compose restart api
```

---

### 4. Caché consumiendo mucha memoria

**Síntoma:** Uso de RAM > 80%, `GC.GetTotalMemory` alto.

**Diagnóstico:**
```bash
# Ver métricas de memoria
curl http://localhost:5000/health | python3 -m json.tool | grep -i memoria
```

**Soluciones:**
```json
// Reducir tamaño del caché en appsettings
{
  "Cache": {
    "DefaultTtlSeconds": 120,
    "MaxSizeItems": 500
  }
}
```

---

### 5. Error 401 Unauthorized en API

**Síntoma:** Requests autenticados retornan 401.

**Diagnóstico:**
1. Verificar que el token no haya expirado (`JwtExpiryMinutes`)
2. Verificar que el `Issuer` y `Audience` coincidan
3. Verificar que la clave JWT sea la misma en cliente y servidor

**Herramienta:** Decodificar token en https://jwt.io para ver claims.

---

### 6. Backups no se ejecutan

**Síntoma:** Tabla `BackupLog` sin registros recientes.

**Verificar:**
```sql
SELECT * FROM BackupLog ORDER BY Fecha DESC LIMIT 5;
SELECT * FROM ConfiguracionBackup;
```

**Solución manual:**
```bash
# Ejecutar backup manual con SQLite
sqlite3 /data/SistemaVentas.db \
  "VACUUM INTO '/backups/manual/backup_$(date +%Y%m%d_%H%M%S).db';"
```

---

## 📊 Comandos de Diagnóstico Rápido

```bash
# Estado general del sistema
curl -s http://localhost:5000/health | python3 -m json.tool

# Logs en tiempo real
docker-compose logs -f api --tail=50

# Uso de recursos
docker stats sistemaventas-api sistemaventas-redis

# Estado de Redis
docker-compose exec redis redis-cli -a $REDIS_PASSWORD INFO stats

# Verificar BD
sqlite3 /data/SistemaVentas.db "PRAGMA integrity_check; PRAGMA page_count;"
```

---

## 📞 Escalación

| Nivel | Responsable | Tiempo Respuesta |
|-------|------------|------------------|
| L1 | Operaciones | 30 min |
| L2 | Desarrollo | 2 horas |
| L3 | Arquitecto | 4 horas |

---

**Versión del documento:** 4.0.0  
**Última actualización:** 2026-07-16
