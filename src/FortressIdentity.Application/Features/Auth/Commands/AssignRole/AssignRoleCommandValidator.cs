using FluentValidation;
using FortressIdentity.Domain.Constants;

namespace FortressIdentity.Application.Features.Auth.Commands.AssignRole;

/// <summary>
/// Validator for AssignRoleCommand.
/// Ensures the role name is valid and exists in the system.
/// </summary>
public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.RoleName)
            .NotEmpty()
            .WithMessage("RoleName is required.")
            .Must(BeAValidRole)
            .WithMessage($"RoleName must be one of the following: {Roles.Admin}, {Roles.User}.");
    }

    /// <summary>
    /// Validates that the role exists in the Roles constants.
    /// </summary>
    private bool BeAValidRole(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return false;
        }

        return roleName == Roles.Admin || roleName == Roles.User;
    }
}
