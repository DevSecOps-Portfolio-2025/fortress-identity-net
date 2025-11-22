# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de solución y proyectos para restaurar dependencias
COPY ["FortressIdentity.sln", "./"]
COPY ["src/FortressIdentity.Domain/FortressIdentity.Domain.csproj", "src/FortressIdentity.Domain/"]
COPY ["src/FortressIdentity.Application/FortressIdentity.Application.csproj", "src/FortressIdentity.Application/"]
COPY ["src/FortressIdentity.Infrastructure/FortressIdentity.Infrastructure.csproj", "src/FortressIdentity.Infrastructure/"]
COPY ["src/FortressIdentity.WebApi/FortressIdentity.WebApi.csproj", "src/FortressIdentity.WebApi/"]

# Restaurar dependencias (se cachean si no cambian los .csproj)
RUN dotnet restore "src/FortressIdentity.WebApi/FortressIdentity.WebApi.csproj"

# Copiar todo el código fuente
COPY . .

# Compilar el proyecto en modo Release
WORKDIR "/src/src/FortressIdentity.WebApi"
RUN dotnet build "FortressIdentity.WebApi.csproj" -c Release -o /app/build

# ============================================
# Stage 2: Publish
# ============================================
FROM build AS publish
RUN dotnet publish "FortressIdentity.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================================
# Stage 3: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Crear usuario no-root para seguridad
RUN addgroup --system --gid 1000 appgroup && \
    adduser --system --uid 1000 --ingroup appgroup --shell /bin/sh appuser

# Copiar los binarios publicados
COPY --from=publish /app/publish .

# Cambiar ownership de los archivos
RUN chown -R appuser:appgroup /app

# Cambiar a usuario no-root
USER appuser

# Exponer puertos HTTP y HTTPS
EXPOSE 8080
EXPOSE 8443

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Punto de entrada
ENTRYPOINT ["dotnet", "FortressIdentity.WebApi.dll"]
