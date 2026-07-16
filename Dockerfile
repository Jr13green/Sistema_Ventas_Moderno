# ============================================================
# Dockerfile para SistemaVentas API
# Multi-stage build para imagen optimizada
# ============================================================

# ── Stage 1: Build ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0-nanoserver-ltsc2025 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["SistemaVentas.API/SistemaVentas.API.csproj",    "SistemaVentas.API/"]
COPY ["SistemaVentas/SistemaVentas.csproj",             "SistemaVentas/"]

RUN dotnet restore "SistemaVentas.API/SistemaVentas.API.csproj" /p:EnableWindowsTargeting=true

# Copiar código fuente y compilar
COPY . .
WORKDIR "/src/SistemaVentas.API"
RUN dotnet build "SistemaVentas.API.csproj" -c Release -o /app/build /p:EnableWindowsTargeting=true

# ── Stage 2: Publish ──────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "SistemaVentas.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false \
    /p:EnableWindowsTargeting=true

# ── Stage 3: Runtime ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0-nanoserver-ltsc2025 AS final

# Variables de entorno de producción
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80;https://+:443
ENV DOTNET_RUNNING_IN_CONTAINER=true

WORKDIR /app

# Directorio de datos (montar como volumen en producción)
RUN mkdir -p /data /backups /logs

COPY --from=publish /app/publish .

# Puerto HTTP y HTTPS
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "SistemaVentas.API.dll"]
