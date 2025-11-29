namespace FortressIdentity.Infrastructure.Authentication;

/// <summary>
/// JWT configuration settings.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// Secret key for signing the JWT token (must be at least 256 bits for HS256).
    /// </summary>
    public string Secret { get; init; } = null!;

    /// <summary>
    /// The issuer of the JWT token (who created and signed the token).
    /// </summary>
    public string Issuer { get; init; } = null!;

    /// <summary>
    /// The audience of the JWT token (who the token is intended for).
    /// </summary>
    public string Audience { get; init; } = null!;

    /// <summary>
    /// Token expiration time in minutes.
    /// </summary>
    public int ExpiryMinutes { get; init; }
}
