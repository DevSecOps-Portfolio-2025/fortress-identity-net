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
}

/// <summary>
/// Response DTO for user registration.
/// </summary>
/// <param name="UserId">The unique identifier of the newly created user</param>
public record RegisterUserResponse(Guid UserId);
