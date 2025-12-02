using FortressIdentity.Application.Common.Interfaces.Authentication;
using FortressIdentity.Application.Common.Interfaces.Persistence;
using FortressIdentity.Domain.Exceptions;
using MediatR;

namespace FortressIdentity.Application.Features.Auth.Commands.Login;

/// <summary>
/// Handler for LoginUserCommand.
/// Implements the business logic for user authentication following CQRS pattern.
/// </summary>
public sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthenticationResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
    }

    /// <summary>
    /// Handles the user authentication process.
    /// </summary>
    /// <param name="request">The login command containing credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with token and user ID</returns>
    /// <exception cref="DomainException">Thrown when credentials are invalid</exception>
    public async Task<AuthenticationResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            throw new DomainException("Invalid credentials.");
        }

        // 2. Verify password using Argon2
        var isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            throw new DomainException("Invalid credentials.");
        }

        // 3. Check if user account is active
        if (!user.IsActive)
        {
            throw new DomainException("User account is inactive.");
        }

        // 4. Check if Two-Factor Authentication is enabled
        if (user.IsTwoFactorEnabled)
        {
            // User has MFA enabled - require code verification
            return new AuthenticationResponse
            {
                Token = null,
                UserId = user.Id.ToString(),
                RequiresTwoFactor = true,
                Message = "Two-factor authentication code required. Please verify with your authenticator app."
            };
        }

        // 5. Generate JWT token (MFA not enabled)
        var token = _jwtTokenGenerator.GenerateToken(user);

        // 6. Return authentication response
        return new AuthenticationResponse
        {
            Token = token,
            UserId = user.Id.ToString(),
            RequiresTwoFactor = false
        };
    }
}
