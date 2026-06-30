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
