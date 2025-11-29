using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FortressIdentity.WebApi.Controllers;

/// <summary>
/// User management endpoints.
/// </summary>
[Authorize]
public class UsersController : ApiControllerBase
{
    /// <summary>
    /// Gets the current authenticated user's information.
    /// </summary>
    /// <returns>User information from JWT claims</returns>
    /// <response code="200">Returns the authenticated user's email</response>
    /// <response code="401">User is not authenticated</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserMeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<UserMeResponse> GetMe()
    {
        // Extract user information from JWT claims
        var email = User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst("email")?.Value;

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        var firstName = User.FindFirst(ClaimTypes.GivenName)?.Value;
        var lastName = User.FindFirst(ClaimTypes.Surname)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized(new { message = "User email not found in token." });
        }

        var message = $"Hola, {email}";

        return Ok(new UserMeResponse(
            Message: message,
            Email: email,
            UserId: userId,
            FirstName: firstName,
            LastName: lastName
        ));
    }
}

/// <summary>
/// Response DTO for the /me endpoint.
/// </summary>
/// <param name="Message">Greeting message</param>
/// <param name="Email">User's email address</param>
/// <param name="UserId">User's unique identifier</param>
/// <param name="FirstName">User's first name</param>
/// <param name="LastName">User's last name</param>
public record UserMeResponse(
    string Message,
    string? Email,
    string? UserId,
    string? FirstName,
    string? LastName
);
