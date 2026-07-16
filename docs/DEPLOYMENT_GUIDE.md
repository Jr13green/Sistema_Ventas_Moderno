# 🚀 Guía de Instalación en Producción

## Sistema de Ventas Diaria Familiar — FASE 4

---

## 📋 Pre-requisitos

### Hardware Mínimo
| Componente | Mínimo | Recomendado |
|-----------|--------|-------------|
| CPU | 2 cores | 4 cores |
| RAM | 4 GB | 8 GB |
| Disco | 20 GB SSD | 100 GB SSD |
| Red | 100 Mbps | 1 Gbps |

### Software Requerido
- **OS**: Windows Server 2022 / Ubuntu 22.04 LTS
- **.NET 10.0 Runtime** (ASP.NET Core)
- **Docker** 24.0+ y **Docker Compose** v2 (para despliegue containerizado)
- **SQLite** 3.45+ (incluido en el runtime)

---

## 🔧 Instalación sin Docker (Windows Server)

### 1. Instalar .NET 10.0 Runtime
```powershell
# Descargar e instalar .NET 10.0 Hosting Bundle
Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile dotnet-install.ps1
.\dotnet-install.ps1 -Version 10.0 -Runtime aspnetcore
```

### 2. Publicar la aplicación
```powershell
# En la máquina de desarrollo
dotnet publish SistemaVentas.API/SistemaVentas.API.csproj `
  --configuration Release `
  --output C:\Deploy\SistemaVentasAPI `
  /p:EnableWindowsTargeting=true

# Copiar al servidor
Copy-Item -Path C:\Deploy\SistemaVentasAPI -Destination \\servidor\apps\SistemaVentasAPI -Recurse
```

### 3. Configurar variables de entorno
```powershell
# Establecer variables de entorno del sistema (PowerShell como Admin)
[System.Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production", "Machine")
[System.Environment]::SetEnvironmentVariable("Security__JwtSecretKey", "TU_CLAVE_JWT_SECRETA_AQUI", "Machine")
[System.Environment]::SetEnvironmentVariable("Security__EncryptionKeyBase64", "TU_CLAVE_ENCRIPTACION_BASE64", "Machine")
[System.Environment]::SetEnvironmentVariable("Database__ConnectionString", "Data Source=C:\Data\SistemaVentas.db;Journal Mode=WAL;", "Machine")
```

### 4. Instalar como servicio de Windows
```powershell
# Instalar como Windows Service con sc.exe
sc.exe create "SistemaVentasAPI" `
  binPath="C:\apps\SistemaVentasAPI\SistemaVentas.API.exe" `
  DisplayName="Sistema Ventas API" `
  start=auto

sc.exe description "SistemaVentasAPI" "API REST del Sistema de Ventas Diaria Familiar"
sc.exe start "SistemaVentasAPI"
```

### 5. Ejecutar scripts de base de datos
```powershell
# Ejecutar scripts de optimización de BD
sqlite3 C:\Data\SistemaVentas.db < SistemaVentas/Database/Scripts/01_CREATE_INDEXES.sql
sqlite3 C:\Data\SistemaVentas.db < SistemaVentas/Database/Scripts/02_PROCEDURES.sql
sqlite3 C:\Data\SistemaVentas.db < SistemaVentas/Database/Scripts/03_BACKUP_SCHEDULE.sql
```

---

## 🐳 Instalación con Docker

### 1. Preparar el entorno
```bash
# Crear archivo .env con secretos (NO commitear a git)
cat > .env << 'EOF'
JWT_SECRET_KEY=tu_clave_jwt_min_32_caracteres_aqui
ENCRYPTION_KEY=base64_de_tu_clave_encriptacion_aqui
REDIS_PASSWORD=tu_password_redis_seguro
APP_INSIGHTS_CONNECTION=
EOF
```

### 2. Iniciar los servicios
```bash
# Iniciar todos los servicios
docker-compose up -d

# Verificar estado
docker-compose ps
docker-compose logs api --tail=50
```

### 3. Ejecutar scripts de BD
```bash
# Ejecutar scripts de optimización dentro del contenedor
docker-compose exec api sqlite3 /data/SistemaVentas.db < SistemaVentas/Database/Scripts/01_CREATE_INDEXES.sql
```

### 4. Verificar el despliegue
```bash
# Health check
curl http://localhost:5000/health

# Swagger (si está habilitado)
curl http://localhost:5000/swagger
```

---

## 🔑 Gestión de Secretos

### Generar claves de producción
```csharp
// Usar la utilidad incluida en EncryptionService
var jwtKey        = System.Security.Cryptography.RandomNumberGenerator.GetBytes(64);
var encryptionKey = EncryptionService.GenerateKey();

Console.WriteLine($"JWT Secret: {Convert.ToBase64String(jwtKey)}");
Console.WriteLine($"Encryption Key: {encryptionKey}");
```

### .NET User Secrets (desarrollo)
```bash
dotnet user-secrets init --project SistemaVentas.API
dotnet user-secrets set "Security:JwtSecretKey" "tu_clave_desarrollo" --project SistemaVentas.API
dotnet user-secrets set "Security:EncryptionKeyBase64" "tu_clave_base64" --project SistemaVentas.API
```

---

## 📊 Verificación post-instalación

```bash
# 1. Health check
curl -s http://localhost:5000/health | python3 -m json.tool

# 2. Test de autenticación
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"numero":"99887766","password":"TuPassword123"}'

# 3. Verificar métricas de caché (logs)
docker-compose logs api | grep "cache"
```

---

## 🔄 Actualización

```bash
# 1. Hacer backup antes de actualizar
./scripts/backup-now.sh

# 2. Desplegar nueva versión (Docker)
docker-compose pull
docker-compose up -d --no-deps api

# 3. Verificar salud post-actualización
curl http://localhost:5000/health
```

---

**Versión del documento:** 4.0.0  
**Última actualización:** 2026-07-16  
**Tiempo estimado de instalación:** 30-60 minutos
