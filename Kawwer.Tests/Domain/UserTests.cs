using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Tests.Domain;

public sealed class UserTests
{
    private static User CreateUser() => new("ali", "ali@example.com", "hash", "Ali", "Ben");

    [Fact]
    public void NewUser_StartsAtFullReputation_AndVeryReliableBadge()
    {
        var user = CreateUser();
        Assert.Equal(100m, user.Reputation);
        Assert.Equal(ReliabilityBadge.VeryReliable, user.GetReliabilityBadge());
    }

    [Theory]
    [InlineData(95, ReliabilityBadge.VeryReliable)]
    [InlineData(80, ReliabilityBadge.Reliable)]
    [InlineData(60, ReliabilityBadge.OccasionallyCancels)]
    [InlineData(40, ReliabilityBadge.OftenLate)]
    [InlineData(10, ReliabilityBadge.FrequentNoShow)]
    public void Badge_ReflectsReputation(int target, ReliabilityBadge expected)
    {
        var user = CreateUser();
        user.AdjustReputation(target - 100); // move from 100 down to target
        Assert.Equal(expected, user.GetReliabilityBadge());
    }

    [Fact]
    public void AdjustReputation_IsClampedBetweenZeroAndHundred()
    {
        var user = CreateUser();
        user.AdjustReputation(50);
        Assert.Equal(100m, user.Reputation);

        user.AdjustReputation(-500);
        Assert.Equal(0m, user.Reputation);
    }

    [Fact]
    public void NewUser_HasNotCompletedOnboarding()
    {
        var user = CreateUser();
        Assert.False(user.OnboardingCompleted);
        Assert.Null(user.OnboardingCompletedAt);
    }

    [Fact]
    public void CompleteOnboarding_StoresAnswersAndMarksCompleted()
    {
        var user = CreateUser();
        var dob = new DateOnly(1998, 5, 20);

        user.CompleteOnboarding(dob, PreferredPosition.Midfielder, PreferredFoot.Left);

        Assert.True(user.OnboardingCompleted);
        Assert.NotNull(user.OnboardingCompletedAt);
        Assert.Equal(dob, user.BirthDate);
        Assert.Equal(PreferredPosition.Midfielder, user.PreferredPosition);
        Assert.Equal(PreferredFoot.Left, user.PreferredFoot);
    }

    [Fact]
    public void CompleteOnboarding_IsIdempotent_AndDoesNotResetTheCompletionStamp()
    {
        var user = CreateUser();
        user.CompleteOnboarding(new DateOnly(1998, 5, 20), PreferredPosition.Midfielder, PreferredFoot.Left);
        var firstStamp = user.OnboardingCompletedAt;

        // A replayed/retried command may update the answers, but must not move the completion time.
        user.CompleteOnboarding(new DateOnly(2000, 1, 1), PreferredPosition.Forward, PreferredFoot.Right);

        Assert.Equal(firstStamp, user.OnboardingCompletedAt);
        Assert.Equal(PreferredPosition.Forward, user.PreferredPosition);
        Assert.Equal(PreferredFoot.Right, user.PreferredFoot);
    }

    [Fact]
    public void FailedLogins_LockAccountAfterThreshold()
    {
        var user = CreateUser();
        for (var i = 0; i < 5; i++)
        {
            user.RegisterFailedLogin(5, TimeSpan.FromMinutes(15));
        }

        Assert.True(user.IsLockedOut);

        user.RegisterSuccessfulLogin();
        Assert.False(user.IsLockedOut);
    }
}
