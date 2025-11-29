using FortressIdentity.Application.Features.Auth;
using FortressIdentity.Application.Features.Auth.Commands.AssignRole;
using FortressIdentity.Application.Features.Auth.Commands.Login;
using FortressIdentity.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FortressIdentity.WebApi.Controllers;

/// <summary>
/// Authentication and authorization endpoints.
/// </summary>
public class AuthController : ApiControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="command">User registration data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The ID of the newly created user</returns>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="409">Email already exists</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterUserResponse>> Register(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var userId = await _sender.Send(command, cancellationToken);

        var response = new RegisterUserResponse(userId);

        return CreatedAtAction(
            actionName: nameof(Register),
            value: response);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="command">User login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token</returns>
    /// <response code="200">Login successful, returns JWT token</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> Login(
        [FromBody] LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(command, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Assigns a role to an existing user.
    /// </summary>
    /// <param name="command">Role assignment data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Role assigned successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="404">User not found</response>
    [HttpPost("assign-role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AssignRole(
        [FromBody] AssignRoleCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);

        return NoContent();
    }
}

/// <summary>
/// Response DTO for user registration.
/// </summary>
/// <param name="UserId">The unique identifier of the newly created user</param>
public record RegisterUserResponse(Guid UserId);
