using Kawwer.Domain.Enums;

namespace Kawwer.Contracts.Users;

public sealed record UserDto(
    Guid Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? ProfilePictureUrl,
    DateOnly? BirthDate,
    PreferredPosition? PreferredPosition,
    PreferredFoot? PreferredFoot,
    int? SkillLevel,
    decimal Reputation,
    ReliabilityBadge ReliabilityBadge,
    ProfileVisibility Visibility,
    DateTime CreatedAt);

public sealed record UserSummaryDto(
    Guid Id,
    string Username,
    string FirstName,
    string LastName,
    string? ProfilePictureUrl,
    decimal Reputation,
    ReliabilityBadge ReliabilityBadge);

public sealed record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    DateOnly? BirthDate,
    PreferredPosition? PreferredPosition,
    PreferredFoot? PreferredFoot,
    int? SkillLevel,
    ProfileVisibility Visibility);

public sealed record UpdateDeviceTokenRequest(string? DeviceToken);
