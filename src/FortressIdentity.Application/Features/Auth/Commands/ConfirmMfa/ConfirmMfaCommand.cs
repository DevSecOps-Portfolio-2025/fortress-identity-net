using MediatR;

namespace FortressIdentity.Application.Features.Auth.Commands.ConfirmMfa;

/// <summary>
/// Command to confirm and activate MFA after scanning the QR code.
/// The user must verify a code to prove they successfully set up the authenticator app.
/// </summary>
/// <param name="Code">6-digit TOTP code from authenticator app</param>
public record ConfirmMfaCommand(string Code) : IRequest<ConfirmMfaResponse>;
