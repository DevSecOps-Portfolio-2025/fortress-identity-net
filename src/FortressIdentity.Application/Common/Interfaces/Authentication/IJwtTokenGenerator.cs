using FortressIdentity.Domain.Entities;

namespace FortressIdentity.Application.Common.Interfaces.Authentication;

/// <summary>
/// Interface for generating JWT tokens.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom to generate the token.</param>
    /// <returns>A signed JWT token string.</returns>
    string GenerateToken(User user);
}
