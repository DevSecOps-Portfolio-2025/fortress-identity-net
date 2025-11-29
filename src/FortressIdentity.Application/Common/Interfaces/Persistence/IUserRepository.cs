using FortressIdentity.Domain.Entities;

namespace FortressIdentity.Application.Common.Interfaces.Persistence;

/// <summary>
/// Repository interface for User entity operations.
/// Follows Repository pattern to abstract data access.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Checks if a user with the specified email exists.
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if email exists, false otherwise</returns>
    Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their email address.
    /// </summary>
    /// <param name="email">Email address to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User entity if found, null otherwise</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their unique identifier.
    /// </summary>
    /// <param name="id">User ID to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User entity if found, null otherwise</returns>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user to the repository.
    /// </summary>
    /// <param name="user">User entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user in the repository.
    /// </summary>
    /// <param name="user">User entity to update</param>
    void Update(User user);

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
