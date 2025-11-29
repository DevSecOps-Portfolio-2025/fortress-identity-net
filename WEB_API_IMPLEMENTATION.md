# 🚀 Exposición de CQRS a través de Web API

## ✅ Implementación Completada

Se ha expuesto el caso de uso `RegisterUser` a través de endpoints REST siguiendo las mejores prácticas de ASP.NET Core.

---

## 📂 Archivos Creados/Modificados

### 1️⃣ **ApiControllerBase.cs** (Nueva)
```csharp
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
```

**Propósito:**
- ✅ Clase base para todos los controllers
- ✅ Centraliza atributos comunes (`[ApiController]`, `[Route]`)
- ✅ Facilita mantenimiento y extensión futura

---

### 2️⃣ **AuthController.cs** (Nueva)

```csharp
public class AuthController : ApiControllerBase
{
    private readonly ISender _sender;

    [HttpPost("register")]
    public async Task<ActionResult<RegisterUserResponse>> Register(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var userId = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Register), new RegisterUserResponse(userId));
    }
}
```

**Características:**
- ✅ Usa `ISender` (interfaz limpia de MediatR)
- ✅ Endpoint: `POST /api/auth/register`
- ✅ Recibe `RegisterUserCommand` directamente en el body
- ✅ Retorna `201 Created` con el `userId`
- ✅ Documentado con XML comments para Swagger
- ✅ Especifica response types (`[ProducesResponseType]`)

**Response DTO:**
```csharp
public record RegisterUserResponse(Guid UserId);
```

---

### 3️⃣ **GlobalExceptionHandler.cs** (Nueva)

Middleware que implementa `IExceptionHandler` (.NET 8+):

```csharp
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            ValidationException => 400 Bad Request,
            InvalidEntityException => 400 Bad Request,
            DomainException (email exists) => 409 Conflict,
            DomainException (other) => 400 Bad Request,
            _ => 500 Internal Server Error
        };
        
        // Retorna ProblemDetails (RFC 7807)
    }
}
```

**Maneja:**
- ✅ `ValidationException` (FluentValidation) → **400 Bad Request**
  - Retorna diccionario con errores por campo
- ✅ `DomainException` (email duplicado) → **409 Conflict**
- ✅ `DomainException` (otras reglas de negocio) → **400 Bad Request**
- ✅ `InvalidEntityException` → **400 Bad Request**
- ✅ Excepciones no controladas → **500 Internal Server Error**
  - Sin exponer detalles sensibles

**Formato de respuesta (Problem Details RFC 7807):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "Password": [
      "Password must be at least 12 characters long.",
      "Password must contain at least one uppercase letter..."
    ]
  }
}
```

---

### 4️⃣ **Program.cs** (Modificado)

**Cambios realizados:**

```csharp
// ✅ Agregado: Registro de Application Layer
builder.Services.AddApplication();

// ✅ Ya existía: Registro de Infrastructure Layer
builder.Services.AddInfrastructure(builder.Configuration);

// ✅ Agregado: Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ✅ Agregado en el pipeline: Middleware de excepciones
app.UseExceptionHandler();
```

**Orden del Pipeline:**
```
1. UseExceptionHandler() → Captura excepciones globales
2. UseSwagger/UseSwaggerUI() → Solo en Development
3. UseHttpsRedirection()
4. UseAuthorization()
5. MapControllers()
```

---

## 🧪 Archivo de Pruebas HTTP

Se creó `Auth.http` con casos de prueba:

### ✅ Caso Exitoso
```http
POST https://localhost:7298/api/auth/register
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "SecureP@ssw0rd123!"
}

# Expected: 201 Created
# Response: { "userId": "guid-here" }
```

### ❌ Validación: Contraseña Débil
```http
{
  "password": "weak"
}

# Expected: 400 Bad Request
# Response: ProblemDetails con errores de validación
```

### ❌ Email Duplicado
```http
# Registrar el mismo email dos veces

# Expected: 409 Conflict
# Response: { "detail": "A user with email '...' already exists." }
```

---

## 🔄 Flujo Completo Request → Response

```
┌─────────────────────────────────────────────────────────────┐
│ 1. HTTP Request                                             │
│    POST /api/auth/register                                  │
│    Body: { firstName, lastName, email, password }           │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. AuthController                                           │
│    _sender.Send(RegisterUserCommand)                        │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. MediatR Pipeline                                         │
│    ┌────────────────────────────────────────────┐          │
│    │ RegisterUserCommandValidator               │          │
│    │ - Valida propiedades                       │          │
│    │ - Si falla → ValidationException           │          │
│    └────────────────────┬───────────────────────┘          │
│                         │ ✅ Valid                          │
│                         ▼                                    │
│    ┌────────────────────────────────────────────┐          │
│    │ RegisterUserCommandHandler                 │          │
│    │ 1. Check email (IUserRepository)           │          │
│    │ 2. Hash password (IPasswordHasher)         │          │
│    │ 3. Create User entity                      │          │
│    │ 4. Save to DB                              │          │
│    │ 5. Return userId                           │          │
│    └────────────────────┬───────────────────────┘          │
└─────────────────────────┼────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│ 4a. Success Path                                            │
│     Return 201 Created                                      │
│     Body: { "userId": "guid" }                              │
└─────────────────────────────────────────────────────────────┘
                         OR
                         ▼
┌─────────────────────────────────────────────────────────────┐
│ 4b. Error Path → GlobalExceptionHandler                    │
│     ├─ ValidationException → 400 Bad Request                │
│     ├─ DomainException (email exists) → 409 Conflict        │
│     ├─ DomainException (other) → 400 Bad Request            │
│     └─ Unknown → 500 Internal Server Error                  │
│                                                              │
│     Returns: ProblemDetails (RFC 7807)                      │
└─────────────────────────────────────────────────────────────┘
```

---

## 📊 Respuestas HTTP por Escenario

| Escenario | Status | Tipo de Respuesta |
|-----------|--------|-------------------|
| Registro exitoso | **201 Created** | `RegisterUserResponse` |
| Validación fallida (FluentValidation) | **400 Bad Request** | `ProblemDetails` con `errors` |
| Email ya existe (DomainException) | **409 Conflict** | `ProblemDetails` |
| Entidad inválida (InvalidEntityException) | **400 Bad Request** | `ProblemDetails` |
| Error no controlado | **500 Internal Server Error** | `ProblemDetails` (sin detalles sensibles) |

---

## 🎯 Ventajas de Esta Implementación

### ✅ Clean Architecture
- Controllers **no conocen** la lógica de negocio
- Dependencias apuntan hacia adentro (Domain ← Application ← Infrastructure ← WebApi)

### ✅ Separation of Concerns
- **Controller:** Recibe request, delega a MediatR, retorna response
- **Handler:** Lógica de negocio pura
- **Middleware:** Manejo centralizado de errores

### ✅ API RESTful
- Uso correcto de status codes
- Respuestas estandarizadas (Problem Details RFC 7807)
- Documentación automática con Swagger

### ✅ Testabilidad
- Controller fácil de testear (mock `ISender`)
- Middleware fácil de testear (mock `HttpContext`)
- Excepciones bien tipadas

### ✅ Extensibilidad
- Agregar nuevos endpoints = crear handler + registrar en controller
- Agregar validaciones = crear/modificar validator
- Agregar logging = pipeline behavior de MediatR

---

## 🚀 Próximos Pasos

1. **Tests de Integración:**
   - Probar endpoint completo con WebApplicationFactory
   - Verificar status codes y respuestas

2. **Swagger/OpenAPI:**
   - Mejorar documentación con XML comments
   - Agregar ejemplos de request/response

3. **Seguridad:**
   - Implementar rate limiting
   - Agregar CORS policies
   - Implementar JWT authentication

4. **Logging:**
   - Agregar pipeline behavior para logging
   - Structured logging con Serilog

---

## ✅ Compilación Exitosa

```bash
✓ FortressIdentity.Domain - Compilado exitosamente
✓ FortressIdentity.Application - Compilado exitosamente
✓ FortressIdentity.Infrastructure - Compilado exitosamente
✓ FortressIdentity.WebApi - Compilado exitosamente
✓ Sin errores de compilación
```

La API está lista para recibir requests en el endpoint `POST /api/auth/register` 🎉
