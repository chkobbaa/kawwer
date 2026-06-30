using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Auth;

/// <summary>Revokes a refresh token on logout. No-op if the token is unknown or already revoked.</summary>
public sealed record LogoutCommand(string RefreshToken) : IRequest<Unit>;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokens, IUnitOfWork unitOfWork)
    {
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(LogoutCommand request, CancellationToken cancellationToken)
    {
        var stored = await _refreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (stored is { RevokedAt: null })
        {
            stored.Revoke();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
