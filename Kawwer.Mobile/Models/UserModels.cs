namespace Kawwer.Mobile.Models;

/// <summary>Display formatting helpers for people's names.</summary>
public static class NameFormat
{
    /// <summary>Uppercases the first letter, e.g. "john" -> "John".</summary>
    public static string Capitalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        return char.ToUpperInvariant(trimmed[0]) + trimmed[1..];
    }
}

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public UserDto User { get; set; } = new();
}

/// <summary>Latest published app version + download URL, returned by GET /system/version.</summary>
public sealed class AppVersionDto
{
    public string LatestVersion { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public bool Mandatory { get; set; }
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
    public bool OnboardingCompleted { get; set; }
    public string DisplayFirstName => NameFormat.Capitalize(FirstName);
    public string FullName => $"{NameFormat.Capitalize(FirstName)} {NameFormat.Capitalize(LastName)}".Trim();
    public string Initials => $"{(FirstName.Length > 0 ? char.ToUpperInvariant(FirstName[0]) : ' ')}{(LastName.Length > 0 ? char.ToUpperInvariant(LastName[0]) : ' ')}".Trim();
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
    public string FullName => $"{NameFormat.Capitalize(FirstName)} {NameFormat.Capitalize(LastName)}".Trim();
    public string Initials => $"{(FirstName.Length > 0 ? char.ToUpperInvariant(FirstName[0]) : ' ')}{(LastName.Length > 0 ? char.ToUpperInvariant(LastName[0]) : ' ')}".Trim();
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

public sealed class TeamDto
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
    public string? GoogleMapsUrl { get; set; }
    public string? Notes { get; set; }
    public Guid CreatedBy { get; set; }

    public string SurfaceLabel => Surface switch
    {
        SurfaceType.ArtificialTurf => "Artificial turf",
        SurfaceType.NaturalGrass => "Natural grass",
        _ => "Concrete"
    };

    public string PriceLabel => $"{Price} TND";

    /// <summary>"Field name - Full address", used on the home "next match" hero card.</summary>
    public string NameWithAddress => string.IsNullOrWhiteSpace(Address) ? Name : $"{Name} - {Address}";
}
