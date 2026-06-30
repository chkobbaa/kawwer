namespace Kawwer.Mobile.Models;

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public UserDto User { get; set; } = new();
}

public sealed class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateOnly? BirthDate { get; set; }
    public PreferredPosition? PreferredPosition { get; set; }
    public PreferredFoot? PreferredFoot { get; set; }
    public int? SkillLevel { get; set; }
    public decimal Reputation { get; set; }
    public ReliabilityBadge ReliabilityBadge { get; set; }
    public ProfileVisibility Visibility { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public sealed class UserSummaryDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public decimal Reputation { get; set; }
    public ReliabilityBadge ReliabilityBadge { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Initials => $"{(FirstName.Length > 0 ? FirstName[0] : ' ')}{(LastName.Length > 0 ? LastName[0] : ' ')}".Trim();
}

public sealed class FriendDto
{
    public Guid FriendshipId { get; set; }
    public UserSummaryDto User { get; set; } = new();
    public DateTime FriendsSince { get; set; }
}

public sealed class FriendRequestDto
{
    public Guid FriendshipId { get; set; }
    public UserSummaryDto User { get; set; } = new();
    public bool IsIncoming { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class GroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MemberCount { get; set; }
    public List<UserSummaryDto> Members { get; set; } = new();
}

public sealed class FootballFieldDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int Capacity { get; set; }
    public int MatchDurationMinutes { get; set; }
    public decimal Price { get; set; }
    public decimal ReservationFee { get; set; }
    public SurfaceType Surface { get; set; }
    public bool Indoor { get; set; }
    public bool Parking { get; set; }
    public bool Shower { get; set; }
    public bool Lights { get; set; }
    public string? PhoneNumber { get; set; }
}
