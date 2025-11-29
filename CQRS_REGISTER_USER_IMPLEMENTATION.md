# Implementación de CQRS - Registro de Usuario

## 📋 Resumen

Se ha implementado el patrón **CQRS (Command Query Responsibility Segregation)** para el caso de uso "Registrar Usuario" usando **MediatR**, **FluentValidation** y **Clean Architecture**.

---

## 🏗️ Arquitectura Implementada

```
┌─────────────────────────────────────────────────────────────┐
│                      WebApi Layer                           │
│  Controller → IMediator.Send(RegisterUserCommand)          │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                  Application Layer (CQRS)                   │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │  RegisterUserCommand (DTO)                        │    │
│  │  - FirstName, LastName, Email, Password           │    │
│  │  - Implements: IRequest<Guid>                     │    │
│  └────────────────────────────────────────────────────┘    │
│                         │                                    │
│                         ▼                                    │
│  ┌────────────────────────────────────────────────────┐    │
│  │  RegisterUserCommandValidator (FluentValidation)  │    │
│  │  - Validates command before execution             │    │
│  │  - Strong password regex, email format, etc.      │    │
│  └────────────────────────────────────────────────────┘    │
│                         │                                    │
│                         ▼                                    │
│  ┌────────────────────────────────────────────────────┐    │
│  │  RegisterUserCommandHandler                       │    │
│  │  - Implements: IRequestHandler<Command, Guid>     │    │
│  │  - Business Logic:                                │    │
│  │    1. Check email uniqueness                      │    │
│  │    2. Hash password (IPasswordHasher)             │    │
│  │    3. Create User entity (Rich Domain Model)      │    │
│  │    4. Save to DB (IUserRepository)                │    │
│  │    5. Return User.Id                              │    │
│  └────────────────────────────────────────────────────┘    │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                Infrastructure Layer                          │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │  UserRepository : IUserRepository                 │    │
│  │  - ExistsAsync(email)                             │    │
│  │  - AddAsync(user)                                 │    │
│  │  - SaveChangesAsync()                             │    │
│  └────────────────────────────────────────────────────┘    │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │  Argon2PasswordHasher : IPasswordHasher           │    │
│  │  - Hash(password)                                 │    │
│  │  - Verify(password, hash)                         │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔗 ¿Cómo MediatR Conecta Estas Piezas?

### 1️⃣ **Registro en DI Container** (`DependencyInjection.cs`)

```csharp
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
});
```

**¿Qué hace?**
- Escanea el ensamblado de `Application`
- Encuentra todas las clases que implementan `IRequestHandler<TRequest, TResponse>`
- Las registra automáticamente en el contenedor de DI

### 2️⃣ **Flujo de Ejecución**

```csharp
// En el Controller (WebApi)
var command = new RegisterUserCommand("John", "Doe", "john@example.com", "SecureP@ssw0rd123!");
Guid userId = await _mediator.Send(command);
```

**MediatR hace lo siguiente:**

1. **Recibe el comando** → `RegisterUserCommand`
2. **Busca el handler registrado** → `RegisterUserCommandHandler`
3. **Ejecuta la validación** (si está configurado FluentValidation pipeline)
   - `RegisterUserCommandValidator` valida el comando
   - Si falla → lanza `ValidationException`
4. **Ejecuta el handler** → `RegisterUserCommandHandler.Handle()`
5. **Retorna el resultado** → `Guid` (ID del usuario)

### 3️⃣ **Pipeline de MediatR**

```
Command → Validator → Handler → Response
   ↓          ↓           ↓         ↓
 DTO     FluentVal   Business   Result
              ↓           ↓
         Fails?      Success?
              ↓           ↓
        Exception    Return Guid
```

---

## 📂 Archivos Creados

### ✅ Application Layer

1. **`RegisterUserCommand.cs`**
   - DTO inmutable (`record`)
   - Implementa `IRequest<Guid>`
   - Propiedades: `FirstName`, `LastName`, `Email`, `Password`

2. **`RegisterUserCommandValidator.cs`**
   - Hereda de `AbstractValidator<RegisterUserCommand>`
   - Validaciones:
     - FirstName/LastName: No vacío, max 100 caracteres
     - Email: Formato válido
     - Password: Min 12 caracteres, regex fuerte (mayúscula, minúscula, número, especial)

3. **`RegisterUserCommandHandler.cs`**
   - Implementa `IRequestHandler<RegisterUserCommand, Guid>`
   - Inyecta `IUserRepository` y `IPasswordHasher`
   - Lógica de negocio:
     1. Verifica email duplicado
     2. Hashea contraseña
     3. Crea entidad `User`
     4. Guarda en BD
     5. Retorna `user.Id`

4. **`IUserRepository.cs`** (Interfaz)
   - `ExistsAsync(email)` - Verifica existencia de email
   - `AddAsync(user)` - Agrega usuario
   - `SaveChangesAsync()` - Persiste cambios

5. **`DependencyInjection.cs`**
   - Registra MediatR
   - Registra FluentValidation

### ✅ Infrastructure Layer

6. **`UserRepository.cs`**
   - Implementa `IUserRepository`
   - Usa `ApplicationDbContext` (EF Core)
   - Implementación concreta de persistencia

7. **`DependencyInjection.cs`** (actualizado)
   - Registra `IUserRepository → UserRepository`
   - Registra `IPasswordHasher → Argon2PasswordHasher`

---

## 🚀 Uso del Caso de Uso

### Ejemplo en un Controller (WebApi)

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<Guid>> Register(
        [FromBody] RegisterUserCommand command)
    {
        try
        {
            var userId = await _mediator.Send(command);
            return Ok(userId);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
    }
}
```

---

## 🔐 Validación de Contraseña Fuerte

El validador usa una **expresión regular robusta**:

```regex
^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{}|;:,.<>?]).{12,}$
```

**Requisitos:**
- ✅ Al menos 12 caracteres
- ✅ Al menos 1 mayúscula
- ✅ Al menos 1 minúscula
- ✅ Al menos 1 número
- ✅ Al menos 1 carácter especial

**Ejemplo válido:** `SecureP@ssw0rd123!`

---

## 🧪 Próximos Pasos

1. **Tests Unitarios:**
   - `RegisterUserCommandValidatorTests`
   - `RegisterUserCommandHandlerTests`

2. **Integración en WebApi:**
   - Crear `AuthController`
   - Configurar middleware de validación global

3. **Queries (CQRS completo):**
   - `GetUserByIdQuery`
   - `LoginQuery`

---

## 📚 Beneficios del Patrón CQRS

| Beneficio | Descripción |
|-----------|-------------|
| **Separación de Responsabilidades** | Los comandos modifican estado, las queries lo consultan |
| **Testabilidad** | Cada handler es una unidad independiente |
| **Escalabilidad** | Comandos y queries pueden escalar de forma independiente |
| **Mantenibilidad** | Código organizado por casos de uso |
| **Validación Centralizada** | FluentValidation en un solo lugar |

---

## 🎯 Conclusión

Se implementó un **patrón CQRS completo** para el registro de usuarios, siguiendo:

- ✅ **Clean Architecture** (Domain, Application, Infrastructure)
- ✅ **CQRS** (Command + Handler)
- ✅ **Dependency Inversion** (Interfaces en Application, implementaciones en Infrastructure)
- ✅ **Rich Domain Model** (Entidad `User` con validaciones)
- ✅ **Seguridad** (Argon2 password hashing)
- ✅ **Validación Robusta** (FluentValidation con regex fuerte)

**MediatR actúa como mediador**, desacoplando el controlador del handler y permitiendo pipelines extensibles (logging, caching, validación, etc.).
