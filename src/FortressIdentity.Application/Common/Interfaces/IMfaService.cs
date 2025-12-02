namespace FortressIdentity.Application.Common.Interfaces;

/// <summary>
/// Service for Multi-Factor Authentication (MFA) operations.
/// </summary>
public interface IMfaService
{
    /// <summary>
    /// Generates MFA setup information including a secret key and QR code URI.
    /// </summary>
    /// <param name="userEmail">User's email address for the authenticator app</param>
    /// <param name="issuer">Issuer name (application name)</param>
    /// <returns>A tuple containing the secret key and QR code URI</returns>
    (string SecretKey, string QrCodeUri) GenerateSetupInfo(string userEmail, string issuer = "FortressIdentity");

    /// <summary>
    /// Verifies a TOTP code against a secret key.
    /// </summary>
    /// <param name="secret">The user's TOTP secret key</param>
    /// <param name="code">The 6-digit code to verify</param>
    /// <returns>True if the code is valid, false otherwise</returns>
    bool VerifyCode(string secret, string code);
}
