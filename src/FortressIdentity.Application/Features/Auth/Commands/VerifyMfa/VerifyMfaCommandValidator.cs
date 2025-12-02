using FluentValidation;

namespace FortressIdentity.Application.Features.Auth.Commands.VerifyMfa;

/// <summary>
/// Validator for VerifyMfaCommand.
/// Implements validation rules for MFA verification.
/// </summary>
public sealed class VerifyMfaCommandValidator : AbstractValidator<VerifyMfaCommand>
{
    public VerifyMfaCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Two-factor authentication code is required.")
            .Length(6)
            .WithMessage("Two-factor authentication code must be 6 digits.")
            .Matches(@"^\d{6}$")
            .WithMessage("Two-factor authentication code must contain only digits.");
    }
}
