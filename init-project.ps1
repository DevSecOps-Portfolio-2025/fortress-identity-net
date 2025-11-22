# ============================================
# Fortress Identity - Clean Architecture Setup
# PowerShell Script para .NET 8
# ============================================

Write-Host "🏰 Inicializando Fortress Identity con Clean Architecture..." -ForegroundColor Cyan

# Crear la solución
Write-Host "`n📦 Creando solución..." -ForegroundColor Yellow
dotnet new sln -n FortressIdentity

# Crear carpeta src
New-Item -ItemType Directory -Force -Path "src" | Out-Null

# ============================================
# DOMAIN LAYER - Núcleo puro sin dependencias
# ============================================
Write-Host "`n🎯 Creando Domain Layer (Núcleo puro)..." -ForegroundColor Yellow
dotnet new classlib -n FortressIdentity.Domain -o src/FortressIdentity.Domain -f net8.0
dotnet sln add src/FortressIdentity.Domain/FortressIdentity.Domain.csproj

# ============================================
# APPLICATION LAYER - Casos de uso y lógica de negocio
# ============================================
Write-Host "`n⚙️ Creando Application Layer..." -ForegroundColor Yellow
dotnet new classlib -n FortressIdentity.Application -o src/FortressIdentity.Application -f net8.0
dotnet sln add src/FortressIdentity.Application/FortressIdentity.Application.csproj

# Application depende de Domain
dotnet add src/FortressIdentity.Application/FortressIdentity.Application.csproj reference src/FortressIdentity.Domain/FortressIdentity.Domain.csproj

# ============================================
# INFRASTRUCTURE LAYER - Implementaciones concretas
# ============================================
Write-Host "`n🔧 Creando Infrastructure Layer..." -ForegroundColor Yellow
dotnet new classlib -n FortressIdentity.Infrastructure -o src/FortressIdentity.Infrastructure -f net8.0
dotnet sln add src/FortressIdentity.Infrastructure/FortressIdentity.Infrastructure.csproj

# Infrastructure depende de Application y Domain
dotnet add src/FortressIdentity.Infrastructure/FortressIdentity.Infrastructure.csproj reference src/FortressIdentity.Application/FortressIdentity.Application.csproj
dotnet add src/FortressIdentity.Infrastructure/FortressIdentity.Infrastructure.csproj reference src/FortressIdentity.Domain/FortressIdentity.Domain.csproj

# ============================================
# PRESENTATION LAYER - ASP.NET Core Web API
# ============================================
Write-Host "`n🌐 Creando Web API Layer..." -ForegroundColor Yellow
dotnet new webapi -n FortressIdentity.WebApi -o src/FortressIdentity.WebApi -f net8.0 --use-controllers
dotnet sln add src/FortressIdentity.WebApi/FortressIdentity.WebApi.csproj

# WebApi depende de Application e Infrastructure
dotnet add src/FortressIdentity.WebApi/FortressIdentity.WebApi.csproj reference src/FortressIdentity.Application/FortressIdentity.Application.csproj
dotnet add src/FortressIdentity.WebApi/FortressIdentity.WebApi.csproj reference src/FortressIdentity.Infrastructure/FortressIdentity.Infrastructure.csproj

# ============================================
# Instalación de paquetes NuGet esenciales
# ============================================
Write-Host "`n📚 Instalando paquetes NuGet..." -ForegroundColor Yellow

# Application Layer - MediatR para CQRS
dotnet add src/FortressIdentity.Application/FortressIdentity.Application.csproj package MediatR
dotnet add src/FortressIdentity.Application/FortressIdentity.Application.csproj package FluentValidation
dotnet add src/FortressIdentity.Application/FortressIdentity.Application.csproj package FluentValidation.DependencyInjectionExtensions

# Infrastructure Layer - Entity Framework Core
dotnet add src/FortressIdentity.Infrastructure/FortressIdentity.Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add src/FortressIdentity.Infrastructure/FortressIdentity.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/FortressIdentity.Infrastructure/FortressIdentity.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design

# WebApi Layer
dotnet add src/FortressIdentity.WebApi/FortressIdentity.WebApi.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add src/FortressIdentity.WebApi/FortressIdentity.WebApi.csproj package Swashbuckle.AspNetCore

# ============================================
# Limpieza de archivos por defecto
# ============================================
Write-Host "`n🧹 Limpiando archivos generados por defecto..." -ForegroundColor Yellow
Remove-Item src/FortressIdentity.Domain/Class1.cs -ErrorAction SilentlyContinue
Remove-Item src/FortressIdentity.Application/Class1.cs -ErrorAction SilentlyContinue
Remove-Item src/FortressIdentity.Infrastructure/Class1.cs -ErrorAction SilentlyContinue

# ============================================
# Verificación
# ============================================
Write-Host "`n✅ Estructura creada exitosamente!" -ForegroundColor Green
Write-Host "`n📋 Resumen de dependencias:" -ForegroundColor Cyan
Write-Host "   Domain       -> Sin dependencias (Núcleo puro)" -ForegroundColor White
Write-Host "   Application  -> Domain" -ForegroundColor White
Write-Host "   Infrastructure -> Application + Domain" -ForegroundColor White
Write-Host "   WebApi       -> Application + Infrastructure" -ForegroundColor White

Write-Host "`n🚀 Ejecuta 'dotnet build' para compilar la solución" -ForegroundColor Magenta
Write-Host "🐳 Ejecuta 'docker-compose up --build' para levantar la infraestructura" -ForegroundColor Magenta
