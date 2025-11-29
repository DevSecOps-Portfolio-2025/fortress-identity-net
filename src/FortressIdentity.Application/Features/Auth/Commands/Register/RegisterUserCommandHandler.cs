using FortressIdentity.Application.Common.Interfaces.Authentication;
using FortressIdentity.Application.Common.Interfaces.Persistence;
using FortressIdentity.Domain.Entities;
using FortressIdentity.Domain.Exceptions;
using MediatR;

namespace FortressIdentity.Application.Features.Auth.Commands.Register;

/// <summary>
/// Handler for RegisterUserCommand.
/// Implements the business logic for user registration following CQRS pattern.
/// </summary>
public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    /// <summary>
    /// Handles the user registration process.
    /// </summary>
    /// <param name="request">The registration command containing user data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The ID of the newly created user</returns>
    /// <exception cref="DomainException">Thrown when email already exists</exception>
    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Check if email already exists in the database
        var emailExists = await _userRepository.ExistsAsync(request.Email, cancellationToken);

        if (emailExists)
        {
            throw new DomainException($"A user with email '{request.Email}' already exists.");
        }

        // 2. Hash the password using Argon2
        var passwordHash = _passwordHasher.Hash(request.Password);

        // 3. Create the User entity using the rich domain model constructor
        // This ensures all domain validations are enforced
        var user = new User(
            firstName: request.FirstName,
            lastName: request.LastName,
            email: request.Email,
            passwordHash: passwordHash,
            roles: new List<string> { "User" } // Default role
        );

        // 4. Add the user to the database
        await _userRepository.AddAsync(user, cancellationToken);

        // 5. Save changes to the database
        await _userRepository.SaveChangesAsync(cancellationToken);

        // 6. Return the ID of the newly created user
        return user.Id;
    }
}
