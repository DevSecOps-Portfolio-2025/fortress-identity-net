using MediatR;

namespace FortressIdentity.Application.Features.Auth.Commands.Login;

/// <summary>
/// Command to authenticate a user and generate an access token.
/// Implements CQRS pattern - represents the intent to login.
/// </summary>
/// <param name="Email">User's email address</param>
/// <param name="Password">User's plaintext password</param>
public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<AuthenticationResponse>;
