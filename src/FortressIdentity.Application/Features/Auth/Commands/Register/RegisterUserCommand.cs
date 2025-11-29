using MediatR;

namespace FortressIdentity.Application.Features.Auth.Commands.Register;

/// <summary>
/// Command to register a new user in the system.
/// Implements CQRS pattern - represents the intent to create a user.
/// </summary>
/// <param name="FirstName">User's first name</param>
/// <param name="LastName">User's last name</param>
/// <param name="Email">User's email address (must be unique)</param>
/// <param name="Password">User's plaintext password (will be hashed before storage)</param>
public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password
) : IRequest<Guid>;
