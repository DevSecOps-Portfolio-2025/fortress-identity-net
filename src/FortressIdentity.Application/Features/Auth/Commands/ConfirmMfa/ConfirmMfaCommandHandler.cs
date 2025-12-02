using FortressIdentity.Application.Common.Interfaces;
using FortressIdentity.Application.Common.Interfaces.Persistence;
using FortressIdentity.Domain.Exceptions;
using MediatR;

namespace FortressIdentity.Application.Features.Auth.Commands.ConfirmMfa;

/// <summary>
/// Handler for ConfirmMfaCommand.
/// Verifies the TOTP code and enables MFA for the authenticated user.
/// </summary>
public sealed class ConfirmMfaCommandHandler : IRequestHandler<ConfirmMfaCommand, ConfirmMfaResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IMfaService _mfaService;
    private readonly ICurrentUserService _currentUserService;

    public ConfirmMfaCommandHandler(
        IUserRepository userRepository,
        IMfaService mfaService,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mfaService = mfaService ?? throw new ArgumentNullException(nameof(mfaService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    /// <summary>
    /// Handles the MFA confirmation process.
    /// </summary>
    /// <param name="request">The confirm MFA command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Confirmation response</returns>
    public async Task<ConfirmMfaResponse> Handle(ConfirmMfaCommand request, CancellationToken cancellationToken)
    {
        // 1. Get the authenticated user ID
        var userId = _currentUserService.GetUserId();

        if (!userId.HasValue)
        {
            throw new DomainException("User is not authenticated.");
        }

        // 2. Get the user from repository
        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);

        if (user == null)
        {
            throw new DomainException("User not found.");
        }

        // 3. Check if a secret has been set up (from EnableMfa endpoint)
        if (string.IsNullOrWhiteSpace(user.TwoFactorSecret))
        {
            throw new DomainException("MFA setup not initiated. Please call the setup endpoint first.");
        }

        // 4. Check if MFA is already enabled
        if (user.IsTwoFactorEnabled)
        {
            throw new DomainException("Two-Factor Authentication is already enabled.");
        }

        // 5. Verify the TOTP code
        var isCodeValid = _mfaService.VerifyCode(user.TwoFactorSecret, request.Code);

        if (!isCodeValid)
        {
            throw new DomainException("Invalid two-factor authentication code. Please try again.");
        }

        // 6. Enable MFA (the secret is already saved from EnableMfa)
        user.EnableTwoFactor();

        // 7. Save the updated user
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // 8. Return success response
        return new ConfirmMfaResponse
        {
            Success = true,
            Message = "Two-Factor Authentication has been successfully enabled for your account."
        };
    }
}
