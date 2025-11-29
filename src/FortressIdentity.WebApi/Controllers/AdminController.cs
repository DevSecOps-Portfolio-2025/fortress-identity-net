using FortressIdentity.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FortressIdentity.WebApi.Controllers;

/// <summary>
/// Admin-only endpoints protected by Role-Based Access Control (RBAC).
/// Only users with the Admin role can access these endpoints.
/// </summary>
[Authorize(Roles = Roles.Admin)]
public class AdminController : ApiControllerBase
{
    /// <summary>
    /// Admin dashboard endpoint.
    /// Returns a welcome message with the authenticated user's email.
    /// </summary>
    /// <returns>Welcome message for the administrator</returns>
    /// <response code="200">Returns welcome message with user information</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User does not have the Admin role</response>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public ActionResult<DashboardResponse> GetDashboard()
    {
        // Get the authenticated user's email from claims
        var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";

        var response = new DashboardResponse(
            Message: $"Bienvenido al panel de administración, {userEmail}",
            UserEmail: userEmail,
            UserId: userId
        );

        return Ok(response);
    }
}

/// <summary>
/// Response DTO for the admin dashboard.
/// </summary>
/// <param name="Message">Welcome message</param>
/// <param name="UserEmail">Email of the authenticated admin user</param>
/// <param name="UserId">ID of the authenticated admin user</param>
public record DashboardResponse(
    string Message,
    string UserEmail,
    string UserId
);
