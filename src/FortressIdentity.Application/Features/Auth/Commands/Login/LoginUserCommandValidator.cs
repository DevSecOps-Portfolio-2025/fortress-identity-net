using FluentValidation;

namespace FortressIdentity.Application.Features.Auth.Commands.Login;

/// <summary>
/// Validator for LoginUserCommand.
/// Implements validation rules for user login credentials.
/// </summary>
public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}
