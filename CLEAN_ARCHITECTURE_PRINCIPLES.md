# 🏰 Fortress Identity - Clean Architecture

## 📐 Principios de Clean Architecture Aplicados

### ¿Por qué Domain NO debe referenciar Infrastructure?

#### 🎯 **Principio de Inversión de Dependencias (DIP)**

En Clean Architecture, las dependencias **siempre apuntan hacia adentro**, hacia el núcleo (Domain). El Domain representa las **reglas de negocio puras** y NO debe conocer detalles de implementación.

```
┌─────────────────────────────────────────┐
│         WebApi (Presentation)           │  ← Controladores, DTOs
│  ┌───────────────────────────────────┐  │
│  │      Infrastructure                │  │  ← EF Core, SQL Server, APIs externas
│  │  ┌─────────────────────────────┐  │  │
│  │  │      Application             │  │  │  ← Casos de uso, Validaciones
│  │  │  ┌───────────────────────┐   │  │  │
│  │  │  │      Domain           │   │  │  │  ← Entidades, Value Objects, Interfaces
│  │  │  │   (Núcleo Puro)       │   │  │  │
│  │  │  └───────────────────────┘   │  │  │
│  │  └─────────────────────────────┘  │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘

      Dependencias →→→ HACIA ADENTRO ←←←
```

#### ✅ **Razones Fundamentales:**

1. **Independencia de Frameworks**: El Domain no debe acoplarse a Entity Framework, Dapper, o cualquier ORM.

2. **Testabilidad**: Las entidades de dominio deben ser testeables sin levantar bases de datos o infraestructura.

3. **Reglas de Negocio Puras**: `User`, `Role`, `Permission` son conceptos de negocio, no saben que existen en SQL Server.

4. **Inversión de Control**: Domain define **interfaces** (contratos), Infrastructure las **implementa**.

#### 🔄 **Ejemplo Correcto:**

```csharp
// ✅ Domain: Define el contrato (no conoce SQL Server)
namespace FortressIdentity.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User user);
}

// ✅ Infrastructure: Implementa con EF Core
namespace FortressIdentity.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }
}

// ✅ WebApi: Inyecta la implementación
builder.Services.AddScoped<IUserRepository, UserRepository>();
```

#### ❌ **Anti-Patrón (NUNCA hacer esto):**

```csharp
// ❌ Domain referenciando Infrastructure
using Microsoft.EntityFrameworkCore; // ¡NO!
using FortressIdentity.Infrastructure; // ¡NO!

public class User
{
    [Column("user_id")] // ¡NO! Esto es un detalle de implementación
    public Guid Id { get; set; }
}
```

---

## 📦 Estructura de Carpetas Recomendada

```
FortressIdentity/
│
├── src/
│   ├── FortressIdentity.Domain/
│   │   ├── Entities/           # User, Role, Permission
│   │   ├── ValueObjects/       # Email, Password, Token
│   │   ├── Enums/              # UserStatus, RoleType
│   │   ├── Exceptions/         # DomainException, InvalidEmailException
│   │   └── Repositories/       # Interfaces (IUserRepository)
│   │
│   ├── FortressIdentity.Application/
│   │   ├── Commands/           # CreateUserCommand
│   │   ├── Queries/            # GetUserByIdQuery
│   │   ├── Handlers/           # CreateUserCommandHandler
│   │   ├── DTOs/               # UserDto
│   │   ├── Validators/         # CreateUserCommandValidator
│   │   └── Interfaces/         # IAuthService
│   │
│   ├── FortressIdentity.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── Configurations/ # UserConfiguration (EF)
│   │   │   ├── Repositories/   # UserRepository (implementación)
│   │   │   └── ApplicationDbContext.cs
│   │   ├── Services/           # EmailService, JwtTokenService
│   │   └── DependencyInjection.cs
│   │
│   └── FortressIdentity.WebApi/
│       ├── Controllers/        # UsersController
│       ├── Middlewares/        # ExceptionHandlingMiddleware
│       ├── Filters/            # ValidationFilter
│       └── Program.cs
│
├── Dockerfile
├── docker-compose.yml
├── .dockerignore
└── init-project.ps1
```

---

## 🚀 Comandos de Ejecución

### Inicializar el proyecto:
```powershell
.\init-project.ps1
```

### Compilar la solución:
```powershell
dotnet build
```

### Levantar con Docker:
```bash
docker-compose up --build
```

### Acceder a la API:
- Swagger: http://localhost:5000/swagger
- SQL Server: localhost:1433 (sa / FortressSecure123!)
- Seq Logs: http://localhost:5341

---

## 🎓 Beneficios de esta Arquitectura

| Aspecto | Beneficio |
|---------|-----------|
| **Mantenibilidad** | Cambiar de SQL Server a PostgreSQL solo afecta Infrastructure |
| **Testabilidad** | Domain y Application se testean sin bases de datos reales |
| **Escalabilidad** | Cada capa puede evolucionar independientemente |
| **Equipos Distribuidos** | Frontend, Backend y DevOps trabajan en capas separadas |
| **Migraciones Graduales** | Se puede reemplazar Infrastructure sin tocar el negocio |

---

**Creado por:** Arquitecto de Software Senior
**Framework:** .NET 8 + Clean Architecture + DDD
