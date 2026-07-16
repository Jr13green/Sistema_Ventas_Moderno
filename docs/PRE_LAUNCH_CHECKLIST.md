# ✅ Pre-Launch Checklist

## Sistema de Ventas Diaria Familiar — FASE 4

---

## 🔐 Seguridad

- [ ] Clave JWT generada con 64 bytes aleatorios (`EncryptionService.GenerateKey()`)
- [ ] Clave de encriptación generada con `EncryptionService.GenerateKey()`
- [ ] Swagger deshabilitado en producción (`Api:EnableSwagger: false`)
- [ ] Rate limiting habilitado (`Api:EnableRateLimiting: true`)
- [ ] CORS configurado solo para orígenes autorizados
- [ ] HTTPS habilitado y certificado SSL válido instalado
- [ ] Contraseñas de DB/Redis son fuertes y únicas
- [ ] Archivo `.env` fuera del repositorio de código

## 🗄️ Base de Datos

- [ ] Scripts de índices ejecutados (`01_CREATE_INDEXES.sql`)
- [ ] Vistas y queries optimizadas aplicadas (`02_PROCEDURES.sql`)
- [ ] Configuración de backup aplicada (`03_BACKUP_SCHEDULE.sql`)
- [ ] WAL mode habilitado (`PRAGMA journal_mode = WAL;`)
- [ ] Primer backup manual realizado y verificado
- [ ] Rutina de backup automático programada
- [ ] Integridad verificada (`PRAGMA integrity_check;`)

## 🚀 Performance

- [ ] `Cache:DefaultTtlSeconds` ajustado para producción (≥300)
- [ ] Pool de conexiones de SQLite configurado
- [ ] Paginación configurada en DataGrids (máx 20 items/página)
- [ ] Logs en nivel `Warning` (no `Debug` en producción)

## 🔍 Monitoreo

- [ ] Health check `/health` accesible y retorna `200 OK`
- [ ] Application Insights configurado (si se usa)
- [ ] Alertas de sistema configuradas (disco < 1GB, RAM > 80%)
- [ ] Logs persistidos en volumen externo o sistema central

## 🐳 Docker / Infraestructura

- [ ] `docker-compose.yml` revisado y probado
- [ ] Volúmenes de datos montados fuera del contenedor
- [ ] Política de restart configurada (`unless-stopped`)
- [ ] Health checks del contenedor pasando
- [ ] Imagen taggeada con versión de release

## 📋 Aplicación

- [ ] `appsettings.Production.json` revisado completamente
- [ ] Variables de entorno verificadas en servidor de producción
- [ ] Tests pasando (`dotnet test`)
- [ ] Benchmarks ejecutados y métricas documentadas
- [ ] Endpoints de la API documentados en Swagger
- [ ] CORS testeado desde el cliente WPF

## 📚 Documentación

- [ ] `DEPLOYMENT_GUIDE.md` actualizado
- [ ] `TROUBLESHOOTING_RUNBOOK.md` revisado
- [ ] `DISASTER_RECOVERY_PLAN.md` aprobado
- [ ] Credenciales de acceso documentadas en sistema seguro (Vault/KeePass)

## 🔄 Proceso de Go-Live

1. [ ] Comunicar ventana de mantenimiento a usuarios
2. [ ] Hacer backup completo de datos existentes
3. [ ] Desplegar en staging y ejecutar smoke tests
4. [ ] Promover a producción
5. [ ] Verificar health check post-deploy
6. [ ] Monitorear logs por 30 minutos
7. [ ] Confirmar go-live con stakeholders

---

**Versión:** 4.0.0  
**Fecha:** 2026-07-16  
**Responsable:** Equipo de Desarrollo
