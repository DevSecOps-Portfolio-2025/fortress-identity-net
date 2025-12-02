using MediatR;

namespace FortressIdentity.Application.Features.Auth.Commands.EnableMfa;

/// <summary>
/// Command to initiate MFA setup for the authenticated user.
/// Generates a secret key and QR code URI for Google Authenticator.
/// </summary>
public record EnableMfaCommand : IRequest<EnableMfaResponse>;
