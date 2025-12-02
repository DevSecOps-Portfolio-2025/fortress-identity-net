using FortressIdentity.Application.Common.Interfaces;
using FortressIdentity.Application.Common.Interfaces.Persistence;
using FortressIdentity.Domain.Exceptions;
using MediatR;

namespace FortressIdentity.Application.Features.Auth.Commands.EnableMfa;

/// <summary>
/// Handler for EnableMfaCommand.
/// Generates MFA setup information and saves the secret to the user (without enabling it yet).
/// </summary>
public sealed class EnableMfaCommandHandler : IRequestHandler<EnableMfaCommand, EnableMfaResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IMfaService _mfaService;
    private readonly ICurrentUserService _currentUserService;

    public EnableMfaCommandHandler(
        IUserRepository userRepository,
        IMfaService mfaService,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mfaService = mfaService ?? throw new ArgumentNullException(nameof(mfaService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    /// <summary>
    /// Handles the MFA setup initiation process.
    /// </summary>
    /// <param name="request">The enable MFA command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>MFA setup information including secret key and QR code URI</returns>
    public async Task<EnableMfaResponse> Handle(EnableMfaCommand request, CancellationToken cancellationToken)
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

        // 3. Check if MFA is already enabled
        if (user.IsTwoFactorEnabled)
        {
            throw new DomainException("Two-Factor Authentication is already enabled for this user.");
        }

        // 4. Generate MFA setup information
        var (secretKey, qrCodeUri) = _mfaService.GenerateSetupInfo(user.Email);

        // 5. Setup the secret in the user entity (but don't enable MFA yet)
        // The user needs to verify a code first to complete setup via VerifyMfaCommand
        user.SetupMfaSecret(secretKey);

        // 6. Save the updated user with the secret
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // 7. Return the setup information for QR code generation
        return new EnableMfaResponse
        {
            SecretKey = secretKey,
            QrCodeUri = qrCodeUri
        };
    }
}
