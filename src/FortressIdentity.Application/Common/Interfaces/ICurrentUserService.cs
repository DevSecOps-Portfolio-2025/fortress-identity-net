namespace FortressIdentity.Application.Common.Interfaces;

/// <summary>
/// Service to access the current authenticated user's context.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current authenticated user's ID.
    /// </summary>
    /// <returns>The user ID if authenticated, null otherwise</returns>
    Guid? GetUserId();

    /// <summary>
    /// Gets the current authenticated user's email.
    /// </summary>
    /// <returns>The user email if authenticated, null otherwise</returns>
    string? GetUserEmail();

    /// <summary>
    /// Checks if the user is authenticated.
    /// </summary>
    /// <returns>True if authenticated, false otherwise</returns>
    bool IsAuthenticated();
}
