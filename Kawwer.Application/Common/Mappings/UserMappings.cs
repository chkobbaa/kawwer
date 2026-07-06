using Kawwer.Contracts.Users;
using Kawwer.Domain.Entities;

namespace Kawwer.Application.Common.Mappings;

public static class UserMappings
{
    public static UserDto ToDto(this User user) => new(
        user.Id,
        user.Username,
        user.Email,
        user.FirstName,
        user.LastName,
        user.PhoneNumber,
        user.ProfilePictureUrl,
        user.BirthDate,
        user.PreferredPosition,
        user.PreferredFoot,
        user.SkillLevel,
        user.Reputation,
        user.GetReliabilityBadge(),
        user.Visibility,
        user.CreatedAt,
        user.OnboardingCompleted);

    public static UserSummaryDto ToSummaryDto(this User user) => new(
        user.Id,
        user.Username,
        user.FirstName,
        user.LastName,
        user.ProfilePictureUrl,
        user.Reputation,
        user.GetReliabilityBadge());
}
