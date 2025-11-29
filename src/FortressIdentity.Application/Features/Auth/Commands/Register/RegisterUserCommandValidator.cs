using FluentValidation;
using System.Text.RegularExpressions;

namespace FortressIdentity.Application.Features.Auth.Commands.Register;

/// <summary>
/// Validator for RegisterUserCommand.
/// Implements business rules and constraints for user registration.
/// </summary>
public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    // Regex pattern for strong password:
    // - At least 12 characters
    // - At least one uppercase letter
    // - At least one lowercase letter
    // - At least one digit
    // - At least one special character (!@#$%^&*()_+-=[]{}|;:,.<>?)
    private static readonly Regex StrongPasswordRegex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{}|;:,.<>?]).{12,}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100)
    );

    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required.")
            .MaximumLength(100)
            .WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required.")
            .MaximumLength(100)
            .WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(12)
            .WithMessage("Password must be at least 12 characters long.")
            .Must(BeAStrongPassword)
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character (!@#$%^&*()_+-=[]{}|;:,.<>?).");
    }

    private static bool BeAStrongPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        return StrongPasswordRegex.IsMatch(password);
    }
}
