using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FortressIdentity.Application.Common.Interfaces.Authentication;
using FortressIdentity.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FortressIdentity.Infrastructure.Authentication;

/// <summary>
/// Implementation of JWT token generator using System.IdentityModel.Tokens.Jwt.
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenGenerator"/> class.
    /// </summary>
    /// <param name="jwtOptions">JWT configuration settings.</param>
    public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom to generate the token.</param>
    /// <returns>A signed JWT token string.</returns>
    public string GenerateToken(User user)
    {
        // Create signing credentials using HMAC-SHA256
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            SecurityAlgorithms.HmacSha256);

        // Build the claims for the token
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName)
        };

        // Add role claims if the user has roles
        if (user.Roles != null && user.Roles.Any())
        {
            claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }

        // Create the JWT token
        var securityToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: signingCredentials);

        // Serialize the token to a string
        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }
}
