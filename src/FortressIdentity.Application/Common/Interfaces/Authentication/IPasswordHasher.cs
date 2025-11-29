namespace FortressIdentity.Application.Common.Interfaces.Authentication;

/// <summary>
/// Provides cryptographic password hashing and verification services.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password using a cryptographically secure algorithm.
    /// </summary>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>A string containing the hash and salt information.</returns>
    string Hash(string password);

    /// <summary>
    /// Verifies a password against a previously generated hash.
    /// </summary>
    /// <param name="password">The plaintext password to verify.</param>
    /// <param name="hash">The hash to verify against.</param>
    /// <returns>True if the password matches the hash, false otherwise.</returns>
    bool Verify(string password, string hash);
}
