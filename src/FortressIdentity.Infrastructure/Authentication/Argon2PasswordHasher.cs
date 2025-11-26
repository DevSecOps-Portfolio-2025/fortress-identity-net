using System.Security.Cryptography;
using System.Text;
using FortressIdentity.Application.Common.Interfaces.Authentication;
using Konscious.Security.Cryptography;

namespace FortressIdentity.Infrastructure.Authentication;

/// <summary>
/// Implements password hashing using Argon2id algorithm with security hardening.
/// </summary>
public sealed class Argon2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 16 bytes (128 bits)
    private const int HashSize = 32; // 32 bytes (256 bits)
    private const int Iterations = 4; // Time cost
    private const int MemorySize = 65536; // 64 MB in KB
    private const int DegreeOfParallelism = 4; // Number of threads

    /// <summary>
    /// Hashes a password using Argon2id with cryptographically secure parameters.
    /// </summary>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>A formatted string containing algorithm parameters, salt, and hash in Base64.</returns>
    /// <exception cref="ArgumentException">Thrown when password is null or empty.</exception>
    public string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }

        // Generate cryptographically secure random salt
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Hash the password
        byte[] hash = HashPassword(password, salt);

        // Format: $argon2id$v=19$m=65536,t=4,p=4$saltBase64$hashBase64
        return $"$argon2id$v=19$m={MemorySize},t={Iterations},p={DegreeOfParallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies a password against a previously generated hash.
    /// </summary>
    /// <param name="password">The plaintext password to verify.</param>
    /// <param name="hash">The hash string to verify against.</param>
    /// <returns>True if the password matches the hash, false otherwise.</returns>
    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrEmpty(password))
        {
            return false;
        }

        if (string.IsNullOrEmpty(hash))
        {
            return false;
        }

        try
        {
            // Parse the hash string
            var parts = hash.Split('$', StringSplitOptions.RemoveEmptyEntries);
            
            // Expected format: argon2id, v=19, m=65536,t=4,p=4, saltBase64, hashBase64
            if (parts.Length != 5)
            {
                return false;
            }

            if (parts[0] != "argon2id")
            {
                return false;
            }

            // Extract salt and hash
            byte[] salt = Convert.FromBase64String(parts[3]);
            byte[] expectedHash = Convert.FromBase64String(parts[4]);

            // Hash the provided password with the extracted salt
            byte[] actualHash = HashPassword(password, salt);

            // Constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }
        catch
        {
            // If parsing or verification fails, return false
            return false;
        }
    }

    /// <summary>
    /// Performs the actual Argon2id hashing operation.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <param name="salt">The salt to use.</param>
    /// <returns>The hashed password as a byte array.</returns>
    private static byte[] HashPassword(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };

        return argon2.GetBytes(HashSize);
    }
}
