using FluentValidation;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Auth;
using Kawwer.Domain.Entities;

namespace Kawwer.Application.Features.Auth;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUserRepository _users;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokens,
        IUserRepository users,
        IJwtTokenGenerator jwt,
        IUnitOfWork unitOfWork)
    {
        _refreshTokens = refreshTokens;
        _users = users;
        _jwt = jwt;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> HandleAsync(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var stored = await _refreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (stored is null || !stored.IsActive)
        {
            throw new ForbiddenException("Invalid or expired refresh token.");
        }

        var user = await _users.GetByIdAsync(stored.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new ForbiddenException("Invalid or expired refresh token.");
        }

        // Rotate: revoke the used token and issue a fresh one.
        stored.Revoke();
        var newRefreshToken = _jwt.GenerateRefreshToken();
        _refreshTokens.Add(new RefreshToken(user.Id, newRefreshToken, _jwt.GetRefreshTokenExpiry()));

        var (accessToken, expiresAt) = _jwt.GenerateAccessToken(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, newRefreshToken, expiresAt, user.ToDto());
    }
}
