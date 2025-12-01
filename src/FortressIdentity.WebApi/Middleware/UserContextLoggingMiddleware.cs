using Serilog.Context;
using System.Security.Claims;

namespace FortressIdentity.WebApi.Middleware;

/// <summary>
/// Middleware que enriquece los logs con información del usuario autenticado.
/// Agrega el UserId al contexto de Serilog para que todos los logs de la petición lo incluyan.
/// </summary>
public class UserContextLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Verificar si el usuario está autenticado
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Extraer el UserId del claim (NameIdentifier es el claim estándar para UserId en JWT)
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Agregar el UserId al contexto de logs de Serilog
                // Todos los logs generados durante esta petición incluirán automáticamente el UserId
                using (LogContext.PushProperty("UserId", userId))
                {
                    await _next(context);
                }
                return;
            }
        }

        // Si no hay usuario autenticado o no hay UserId, continuar sin enriquecer
        await _next(context);
    }
}
