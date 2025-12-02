using MediatR;

namespace FortressIdentity.Application.Features.Auth.Commands.VerifyMfa;

/// <summary>
/// Command to verify a two-factor authentication code and complete the login process.
/// </summary>
/// <param name="Email">User's email address</param>
/// <param name="Password">User's plaintext password</param>
/// <param name="Code">6-digit TOTP code from authenticator app</param>
public record VerifyMfaCommand(
    string Email,
    string Password,
    string Code
) : IRequest<AuthenticationResponse>;
