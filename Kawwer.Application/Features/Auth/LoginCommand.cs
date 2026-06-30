using FluentValidation;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Auth;
using Kawwer.Domain.Entities;

namespace Kawwer.Application.Features.Auth;

public sealed record LoginCommand(string UsernameOrEmail, string Password) : IRequest<AuthResponse>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UsernameOrEmail).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);

    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwt,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> HandleAsync(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByUsernameOrEmailAsync(request.UsernameOrEmail, cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new ForbiddenException("Invalid credentials.");
        }

        if (user.IsLockedOut)
        {
            throw new ForbiddenException("Account temporarily locked due to repeated failed logins. Try again later.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.RegisterFailedLogin(MaxFailedAttempts, LockDuration);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new ForbiddenException("Invalid credentials.");
        }

        user.RegisterSuccessfulLogin();

        var (accessToken, expiresAt) = _jwt.GenerateAccessToken(user);
        var refreshToken = _jwt.GenerateRefreshToken();
        _refreshTokens.Add(new RefreshToken(user.Id, refreshToken, _jwt.GetRefreshTokenExpiry()));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, refreshToken, expiresAt, user.ToDto());
    }
}
