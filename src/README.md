# 🛡️ Fortress Identity

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?style=flat&logo=docker)
![Architecture](https://img.shields.io/badge/Architecture-Clean%20%2F%20Onion-green)
![Security](https://img.shields.io/badge/Security-Argon2%20%2B%20AES-red)
![License](https://img.shields.io/badge/License-MIT-blue)

**Fortress Identity** es un sistema de Gestión de Identidades y Accesos (IAM) de nivel empresarial desarrollado con **.NET 8**. 

Este proyecto demuestra la implementación de patrones de arquitectura avanzada y estándares de ciberseguridad modernos para proteger aplicaciones críticas. No es solo un sistema de login; es una fortaleza digital diseñada siguiendo los principios de **Defense in Depth**.

---

## 🚀 Características Principales

### 🔐 Seguridad de Grado Militar
* **Hashing Robusto:** Contraseñas protegidas con **Argon2id** (ganador de la *Password Hashing Competition*), configurado con alto costo de memoria (64MB) para resistencia contra ataques GPU/ASIC.
* **Autenticación JWT Blindada:** Tokens firmados con HMAC-SHA256, validación estricta de audiencia/emisor y `ClockSkew` en cero.
* **MFA (2FA) Estándar:** Autenticación de Doble Factor compatible con **Google Authenticator / Microsoft Authenticator** (TOTP RFC 6238).
* **Protección contra Timing Attacks:** Comparación de hashes y secretos utilizando funciones de tiempo constante.

### 🏗️ Arquitectura de Software
* **Clean Architecture (Onion):** Estricta separación de dependencias (`Domain` -> `Application` -> `Infrastructure` -> `WebApi`).
* **CQRS (Command Query Responsibility Segregation):** Implementado con **MediatR** para separar operaciones de escritura y lectura.
* **Domain-Driven Design (DDD):** Modelos de dominio ricos con encapsulamiento estricto (`private set`), validaciones internas y Value Objects.
* **Inyección de Dependencias (DI):** Gestión de servicios optimizada y desacoplada.

### 👁️ Observabilidad y Calidad
* **Logging Estructurado:** Auditoría completa con **Serilog**, incluyendo enriquecimiento de contexto (`UserId`, `RequestId`) para análisis forense.
* **Manejo Global de Excepciones:** Middleware personalizado que transforma excepciones en respuestas `ProblemDetails` (RFC 7807).
* **Validación Fluida:** Reglas de negocio validadas con **FluentValidation** antes de tocar el dominio.

---

## 🛠️ Tech Stack

* **Core:** C# 12, .NET 8 Web API.
* **Base de Datos:** SQL Server 2022.
* **ORM:** Entity Framework Core 8 (Code-First Migrations).
* **Librerías Clave:** MediatR, FluentValidation, Serilog, Otp.NET, QRCoder, Konscious.Security.Cryptography.
* **Virtualización:** Docker & Docker Compose.
* **Testing/Docs:** Swagger UI (OpenAPI 3.0) con soporte Bearer Auth.

---

## 📂 Estructura del Proyecto

```bash
src/
├── FortressIdentity.Domain/         # Núcleo: Entidades, Excepciones, Constantes (Sin dependencias)
├── FortressIdentity.Application/    # Cerebro: CQRS, DTOs, Validadores, Interfaces (Depende de Domain)
├── FortressIdentity.Infrastructure/ # Músculo: EF Core, Servicios Externos (Auth, Email), Repositorios
└── FortressIdentity.WebApi/         # Entrada: Controllers, Middleware, Configuraciones

⚡ Guía de Inicio Rápido
Prerrequisitos
Docker Desktop instalado y corriendo.

.NET 8 SDK (Opcional, si quieres ejecutar fuera de Docker).

1. Clonar y Configurar
Bash

git clone [https://github.com/TU_USUARIO/fortress-identity.git](https://github.com/TU_USUARIO/fortress-identity.git)
cd fortress-identity
2. Levantar la Infraestructura (Docker)
El proyecto incluye un docker-compose.yml que orquesta la API y SQL Server en una red aislada.

Bash

docker-compose up -d --build
3. Aplicar Migraciones
Dado que la base de datos inicia vacía, necesitas crear las tablas.

Bash

# Opción A: Desde tu máquina local (si tienes .NET SDK)
dotnet ef database update --project src/FortressIdentity.Infrastructure --startup-project src/FortressIdentity.WebApi

# Opción B: Entrando al contenedor (si no tienes .NET SDK local)
docker exec -it fortress-identity-api /bin/bash
cd /app
# (Nota: Requiere configuración adicional de CLI en prod, se recomienda Opción A para desarrollo)
4. Acceder a la Documentación
Abre tu navegador en: 👉 http://localhost:5000/swagger

🧪 Cómo Probar (Flujo Recomendado)
Registro: Usa POST /api/Auth/register para crear un usuario.

Login: Usa POST /api/Auth/login para obtener el JWT.

Setup MFA:

Autorízate en Swagger con el token.

Llama a POST /api/Auth/mfa/setup.

Escanea la URL del QR con Google Authenticator.

Confirma con POST /api/Auth/mfa/confirm.

Login con MFA:

Intenta hacer login de nuevo. Recibirás un mensaje requiriendo 2FA.

Usa POST /api/Auth/login/mfa con tu código TOTP.

Admin Dashboard:

Promuévete a Admin con POST /api/Auth/assign-role.

Accede a GET /api/Admin/dashboard.

🛡️ Auditoría de Seguridad
Los logs se generan en la carpeta /logs y en la consola. Cada petición autenticada incluye automáticamente el UserId en el contexto del log para trazabilidad completa.

JSON

// Ejemplo de Log Estructurado
{
  "Timestamp": "2025-12-01T15:30:45",
  "Level": "Information",
  "Message": "HTTP GET /api/Admin/dashboard responded 200",
  "Properties": {
    "UserId": "998db6b1-84a2-...",
    "UserEmail": "admin@fortress.com",
    "SourceContext": "Serilog.AspNetCore.RequestLoggingMiddleware"
  }
