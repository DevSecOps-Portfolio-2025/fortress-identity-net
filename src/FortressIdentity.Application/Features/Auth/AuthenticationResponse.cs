namespace FortressIdentity.Application.Features.Auth;

/// <summary>
/// Response containing authentication data after successful login.
/// </summary>
public record AuthenticationResponse
{
    /// <summary>
    /// JWT access token. Null when RequiresTwoFactor is true.
    /// </summary>
    public string? Token { get; init; }

    /// <summary>
    /// The unique identifier of the authenticated user.
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Indicates whether the user needs to verify a two-factor authentication code.
    /// </summary>
    public bool RequiresTwoFactor { get; init; }

    /// <summary>
    /// Message to display to the user.
    /// </summary>
    public string? Message { get; init; }
}
