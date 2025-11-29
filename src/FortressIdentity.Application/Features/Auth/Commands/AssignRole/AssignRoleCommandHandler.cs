using FortressIdentity.Application.Common.Interfaces.Persistence;
using FortressIdentity.Domain.Exceptions;
using MediatR;

namespace FortressIdentity.Application.Features.Auth.Commands.AssignRole;

/// <summary>
/// Handler for AssignRoleCommand.
/// Assigns a role to an existing user.
/// </summary>
public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Unit>
{
    private readonly IUserRepository _userRepository;

    public AssignRoleCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <summary>
    /// Handles the AssignRoleCommand by finding the user and adding the role.
    /// </summary>
    /// <param name="request">The command containing UserId and RoleName.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Unit (void equivalent for MediatR).</returns>
    /// <exception cref="DomainException">Thrown when the user is not found.</exception>
    public async Task<Unit> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        // 1. Find the user by ID
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            throw new DomainException($"User with ID '{request.UserId}' not found.");
        }

        // 2. Add the role to the user (domain logic handles validation)
        user.AddRole(request.RoleName);

        // 3. Update the user in the repository
        _userRepository.Update(user);

        // 4. Persist changes to the database
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
