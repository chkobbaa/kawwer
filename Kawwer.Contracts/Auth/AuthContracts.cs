using Kawwer.Contracts.Users;
using Kawwer.Domain.Enums;

namespace Kawwer.Contracts.Auth;

public sealed record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    DateOnly? BirthDate,
    PreferredPosition? PreferredPosition,
    PreferredFoot? PreferredFoot);

public sealed record LoginRequest(string UsernameOrEmail, string Password);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    UserDto User);
