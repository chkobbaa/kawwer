using Kawwer.Domain.Common;
using Kawwer.Domain.Enums;

namespace Kawwer.Domain.Entities;

/// <summary>
/// A registered user of Kawwer. Aggregate root for identity, profile and reputation.
/// </summary>
public class User : AggregateRoot
{
    private const decimal StartingReputation = 100m;

    // Parameterless constructor for EF Core materialization.
    private User()
    {
        Username = string.Empty;
        Email = string.Empty;
        PasswordHash = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    public User(
        string username,
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        DateOnly? birthDate = null,
        PreferredPosition? preferredPosition = null,
        PreferredFoot? preferredFoot = null)
    {
        Id = Guid.NewGuid();
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        PreferredPosition = preferredPosition;
        PreferredFoot = preferredFoot;
        Reputation = StartingReputation;
        CreatedAt = DateTime.UtcNow;
        Status = AccountStatus.Active;
        Visibility = ProfileVisibility.Public;
    }

    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? ProfilePictureUrl { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public PreferredPosition? PreferredPosition { get; private set; }
    public PreferredFoot? PreferredFoot { get; private set; }
    public int? SkillLevel { get; private set; }
    public decimal Reputation { get; private set; }
    public ProfileVisibility Visibility { get; private set; }
    public AccountStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLogin { get; private set; }

    // Stamped the first time the user finishes the first-run onboarding flow. Null means the
    // onboarding flow has not been completed yet, which is what the mobile app uses to decide
    // whether to route a freshly authenticated user into onboarding.
    public DateTime? OnboardingCompletedAt { get; private set; }

    // Lockout handling for failed login attempts.
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }

    // Push notification registration token (FCM).
    public string? DeviceToken { get; private set; }

    public bool IsActive => Status == AccountStatus.Active;

    /// <summary>True once the user has finished the first-run onboarding flow.</summary>
    public bool OnboardingCompleted => OnboardingCompletedAt.HasValue;

    public string FullName => $"{FirstName} {LastName}".Trim();

    public bool IsLockedOut => LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;

    public void UpdateProfile(
        string firstName,
        string lastName,
        string? phoneNumber,
        DateOnly? birthDate,
        PreferredPosition? preferredPosition,
        PreferredFoot? preferredFoot,
        int? skillLevel,
        ProfileVisibility visibility)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        BirthDate = birthDate;
        PreferredPosition = preferredPosition;
        PreferredFoot = preferredFoot;
        SkillLevel = skillLevel;
        Visibility = visibility;
    }

    /// <summary>
    /// Persists the profile answers gathered during the first-run onboarding flow and stamps the
    /// completion time. The stamp is only set once, so replaying the command (e.g. a retried
    /// request) never resets the "already onboarded" state.
    /// </summary>
    public void CompleteOnboarding(
        DateOnly? birthDate,
        PreferredPosition? preferredPosition,
        PreferredFoot? preferredFoot)
    {
        BirthDate = birthDate;
        PreferredPosition = preferredPosition;
        PreferredFoot = preferredFoot;
        OnboardingCompletedAt ??= DateTime.UtcNow;
    }

    public void SetProfilePicture(string url) => ProfilePictureUrl = url;

    public void SetDeviceToken(string? token) => DeviceToken = token;

    public void RegisterSuccessfulLogin()
    {
        LastLogin = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }

    public void RegisterFailedLogin(int maxAttempts, TimeSpan lockDuration)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxAttempts)
        {
            LockedUntil = DateTime.UtcNow.Add(lockDuration);
            FailedLoginAttempts = 0;
        }
    }

    public void ChangePasswordHash(string newPasswordHash) => PasswordHash = newPasswordHash;

    public void SoftDelete() => Status = AccountStatus.Deleted;

    /// <summary>
    /// Adjusts reputation, clamped to the 0..100 range.
    /// </summary>
    public void AdjustReputation(decimal delta)
    {
        Reputation = Math.Clamp(Reputation + delta, 0m, 100m);
    }

    public ReliabilityBadge GetReliabilityBadge()
    {
        return Reputation switch
        {
            >= 90m => ReliabilityBadge.VeryReliable,
            >= 75m => ReliabilityBadge.Reliable,
            >= 55m => ReliabilityBadge.OccasionallyCancels,
            >= 35m => ReliabilityBadge.OftenLate,
            _ => ReliabilityBadge.FrequentNoShow
        };
    }
}
