namespace FortressIdentity.Application.Features.Auth.Commands.ConfirmMfa;

/// <summary>
/// Response after successfully confirming MFA setup.
/// </summary>
public record ConfirmMfaResponse
{
    /// <summary>
    /// Indicates whether MFA was successfully enabled.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Message to display to the user.
    /// </summary>
    public required string Message { get; init; }
}
