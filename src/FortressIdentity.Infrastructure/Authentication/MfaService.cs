using FortressIdentity.Application.Common.Interfaces;
using OtpNet;

namespace FortressIdentity.Infrastructure.Authentication;

/// <summary>
/// Implementation of Multi-Factor Authentication service using TOTP (Time-based One-Time Password).
/// </summary>
public class MfaService : IMfaService
{
    /// <summary>
    /// Generates MFA setup information including a secret key and QR code URI.
    /// </summary>
    /// <param name="userEmail">User's email address for the authenticator app</param>
    /// <param name="issuer">Issuer name (application name)</param>
    /// <returns>A tuple containing the secret key and QR code URI</returns>
    public (string SecretKey, string QrCodeUri) GenerateSetupInfo(string userEmail, string issuer = "FortressIdentity")
    {
        // Generate a random secret key (Base32 encoded, 160 bits = 20 bytes)
        var secretKey = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(secretKey);

        // Create the otpauth URI for QR code generation
        // Format: otpauth://totp/{Issuer}:{Email}?secret={Secret}&issuer={Issuer}
        var qrCodeUri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(userEmail)}?secret={base32Secret}&issuer={Uri.EscapeDataString(issuer)}";

        return (base32Secret, qrCodeUri);
    }

    /// <summary>
    /// Verifies a TOTP code against a secret key.
    /// </summary>
    /// <param name="secret">The user's TOTP secret key (Base32 encoded)</param>
    /// <param name="code">The 6-digit code to verify</param>
    /// <returns>True if the code is valid, false otherwise</returns>
    public bool VerifyCode(string secret, string code)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        try
        {
            // Decode the Base32 secret
            var secretKey = Base32Encoding.ToBytes(secret);

            // Create TOTP instance
            var totp = new Totp(secretKey);

            // Verify the code with a window of ±1 time step (30 seconds each)
            // This allows for slight time differences between server and client
            return totp.VerifyTotp(code, out _, new VerificationWindow(1, 1));
        }
        catch
        {
            // If decoding or verification fails, return false
            return false;
        }
    }
}
