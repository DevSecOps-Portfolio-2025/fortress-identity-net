namespace FortressIdentity.Application.Features.Auth.Commands.EnableMfa;

/// <summary>
/// Response containing MFA setup information.
/// </summary>
public record EnableMfaResponse
{
    /// <summary>
    /// The Base32-encoded secret key to be manually entered in authenticator apps.
    /// </summary>
    public required string SecretKey { get; init; }

    /// <summary>
    /// The otpauth:// URI to generate a QR code for easy setup.
    /// Format: otpauth://totp/FortressIdentity:user@email.com?secret=XXXXX&issuer=FortressIdentity
    /// </summary>
    public required string QrCodeUri { get; init; }

    /// <summary>
    /// Informational message for the user.
    /// </summary>
    public string Message { get; init; } = "Scan the QR code or manually enter the secret key in your authenticator app (e.g., Google Authenticator). Then verify with a code to complete setup.";
}
