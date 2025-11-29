namespace FortressIdentity.Application.Features.Auth;

/// <summary>
/// Response containing authentication data after successful login.
/// </summary>
/// <param name="Token">JWT access token</param>
/// <param name="UserId">The unique identifier of the authenticated user</param>
public record AuthenticationResponse(
    string Token,
    string UserId
);
