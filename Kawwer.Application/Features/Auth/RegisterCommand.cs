using System.Text.RegularExpressions;
using FluentValidation;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Auth;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Auth;

public sealed record RegisterCommand(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    DateOnly? BirthDate,
    PreferredPosition? PreferredPosition,
    PreferredFoot? PreferredFoot) : IRequest<AuthResponse>;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(2).MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Must(HasRequiredComplexity)
            .WithMessage("Password must contain an uppercase letter, a lowercase letter, and a number.");
    }

    private static bool HasRequiredComplexity(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return false;
        }

        return Regex.IsMatch(password, "[A-Z]")
               && Regex.IsMatch(password, "[a-z]")
               && Regex.IsMatch(password, "[0-9]");
    }
}

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
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

    public async Task<AuthResponse> HandleAsync(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _users.UsernameExistsAsync(request.Username, cancellationToken))
        {
            throw new ConflictException("This username is already taken.");
        }

        if (await _users.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var user = new User(
            request.Username,
            request.Email,
            _passwordHasher.Hash(request.Password),
            request.FirstName,
            request.LastName,
            request.BirthDate,
            request.PreferredPosition,
            request.PreferredFoot);

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            user.UpdateProfile(
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.BirthDate,
                request.PreferredPosition,
                request.PreferredFoot,
                null,
                user.Visibility);
        }

        _users.Add(user);

        var (accessToken, expiresAt) = _jwt.GenerateAccessToken(user);
        var refreshToken = _jwt.GenerateRefreshToken();
        _refreshTokens.Add(new RefreshToken(user.Id, refreshToken, _jwt.GetRefreshTokenExpiry()));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, refreshToken, expiresAt, user.ToDto());
    }
}
