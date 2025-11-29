using MediatR;

namespace FortressIdentity.Application.Features.Auth.Commands.AssignRole;

/// <summary>
/// Command to assign a role to an existing user.
/// </summary>
/// <param name="UserId">The unique identifier of the user.</param>
/// <param name="RoleName">The name of the role to assign.</param>
public record AssignRoleCommand(
    Guid UserId,
    string RoleName) : IRequest<Unit>;
