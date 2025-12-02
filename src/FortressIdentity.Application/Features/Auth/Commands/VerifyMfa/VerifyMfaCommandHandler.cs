using FortressIdentity.Application.Common.Interfaces;
using FortressIdentity.Application.Common.Interfaces.Authentication;
using FortressIdentity.Application.Common.Interfaces.Persistence;
using FortressIdentity.Domain.Exceptions;
using MediatR;

namespace FortressIdentity.Application.Features.Auth.Commands.VerifyMfa;

/// <summary>
/// Handler for VerifyMfaCommand.
/// Verifies two-factor authentication code and completes the login process.
/// </summary>
public sealed class VerifyMfaCommandHandler : IRequestHandler<VerifyMfaCommand, AuthenticationResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMfaService _mfaService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public VerifyMfaCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IMfaService mfaService,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _mfaService = mfaService ?? throw new ArgumentNullException(nameof(mfaService));
        _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
    }

    /// <summary>
    /// Handles the MFA verification and authentication process.
    /// </summary>
    /// <param name="request">The verify MFA command containing credentials and TOTP code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token</returns>
    /// <exception cref="DomainException">Thrown when credentials or MFA code are invalid</exception>
    public async Task<AuthenticationResponse> Handle(VerifyMfaCommand request, CancellationToken cancellationToken)
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

        // 4. Verify that MFA is actually enabled for this user
        if (!user.IsTwoFactorEnabled || string.IsNullOrWhiteSpace(user.TwoFactorSecret))
        {
            throw new DomainException("Two-factor authentication is not enabled for this account.");
        }

        // 5. Verify the TOTP code
        var isCodeValid = _mfaService.VerifyCode(user.TwoFactorSecret, request.Code);

        if (!isCodeValid)
        {
            throw new DomainException("Invalid two-factor authentication code.");
        }

        // 6. Generate JWT token (MFA verification successful)
        var token = _jwtTokenGenerator.GenerateToken(user);

        // 7. Return authentication response
        return new AuthenticationResponse
        {
            Token = token,
            UserId = user.Id.ToString(),
            RequiresTwoFactor = false,
            Message = "Login successful with two-factor authentication."
        };
    }
}
