using FluentValidation;

namespace FortressIdentity.Application.Features.Auth.Commands.ConfirmMfa;

/// <summary>
/// Validator for ConfirmMfaCommand.
/// </summary>
public sealed class ConfirmMfaCommandValidator : AbstractValidator<ConfirmMfaCommand>
{
    public ConfirmMfaCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Two-factor authentication code is required.")
            .Length(6)
            .WithMessage("Two-factor authentication code must be 6 digits.")
            .Matches(@"^\d{6}$")
            .WithMessage("Two-factor authentication code must contain only digits.");
    }
}
