using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;
using Kawwer.Domain.Exceptions;

namespace Kawwer.Tests.Domain;

public sealed class MatchLifecycleTests
{
    // A fixed +1 zone stands in for Africa/Tunis (UTC+1, no DST) so tests are host-independent.
    private static readonly TimeZoneInfo PlusOne =
        TimeZoneInfo.CreateCustomTimeZone("Test/PlusOne", TimeSpan.FromHours(1), "PlusOne", "PlusOne");

    private static Match CreateMatch(DateOnly date, TimeOnly start, int durationMinutes = 90)
        => new(
            organizerId: Guid.NewGuid(),
            footballFieldId: Guid.NewGuid(),
            title: "Test Match",
            matchDate: date,
            startTime: start,
            durationMinutes: durationMinutes,
            maxPlayers: 10,
            totalFieldPrice: 90m,
            reservationPaid: 5m,
            visibility: MatchVisibility.Private);

    [Fact]
    public void KickoffInstant_InterpretsWallClockInAppZone()
    {
        // 20:00 local in a +1 zone is 19:00 UTC.
        var match = CreateMatch(new DateOnly(2026, 7, 10), new TimeOnly(20, 0));
        var kickoff = match.KickoffInstant(PlusOne);

        Assert.Equal(new DateTime(2026, 7, 10, 19, 0, 0, DateTimeKind.Utc), kickoff);
    }

    [Fact]
    public void TryExpire_BeforeScheduledEnd_DoesNothing()
    {
        var match = CreateMatch(new DateOnly(2026, 7, 10), new TimeOnly(20, 0));
        match.Publish();

        // 20:30 local kickoff has passed but the 90-minute match ends at 21:30 local (20:30 UTC).
        // At 20:15 UTC it is still in progress.
        var now = new DateTime(2026, 7, 10, 20, 15, 0, DateTimeKind.Utc);

        Assert.False(match.TryExpire(now, PlusOne));
        Assert.Equal(MatchStatus.Published, match.Status);
    }

    [Fact]
    public void TryExpire_AfterScheduledEnd_MarksExpired()
    {
        var match = CreateMatch(new DateOnly(2026, 7, 10), new TimeOnly(20, 0));
        match.Publish();

        // Ends at 21:30 local = 20:30 UTC. At 21:00 UTC it is over.
        var now = new DateTime(2026, 7, 10, 21, 0, 0, DateTimeKind.Utc);

        Assert.True(match.TryExpire(now, PlusOne));
        Assert.Equal(MatchStatus.Expired, match.Status);

        // Idempotent: a second sweep is a no-op.
        Assert.False(match.TryExpire(now, PlusOne));
    }

    [Fact]
    public void TryExpire_DoesNotTouchFinishedOrCancelledMatches()
    {
        var finished = CreateMatch(new DateOnly(2026, 7, 10), new TimeOnly(20, 0));
        finished.Publish();
        finished.Finish();

        var cancelled = CreateMatch(new DateOnly(2026, 7, 10), new TimeOnly(20, 0));
        cancelled.Cancel();

        var wayPast = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        Assert.False(finished.TryExpire(wayPast, PlusOne));
        Assert.Equal(MatchStatus.Finished, finished.Status);
        Assert.False(cancelled.TryExpire(wayPast, PlusOne));
        Assert.Equal(MatchStatus.Cancelled, cancelled.Status);
    }

    [Fact]
    public void ExpiredMatch_RejectsResponsesAndEdits()
    {
        var match = CreateMatch(new DateOnly(2026, 7, 10), new TimeOnly(20, 0));
        match.Publish();
        var invitee = Guid.NewGuid();
        match.Invite(invitee);

        var now = new DateTime(2026, 7, 10, 22, 0, 0, DateTimeKind.Utc);
        Assert.True(match.TryExpire(now, PlusOne));

        Assert.Throws<DomainException>(() => match.Reschedule(new DateOnly(2026, 7, 12), new TimeOnly(20, 0)));
        Assert.Throws<DomainException>(() => match.Cancel());
    }

    [Fact]
    public void Reschedule_ChangesDateTimeAndReportsChange()
    {
        var match = CreateMatch(new DateOnly(2026, 7, 10), new TimeOnly(20, 0));
        match.Publish();

        var changed = match.Reschedule(new DateOnly(2026, 7, 12), new TimeOnly(18, 30));

        Assert.True(changed);
        Assert.Equal(new DateOnly(2026, 7, 12), match.MatchDate);
        Assert.Equal(new TimeOnly(18, 30), match.StartTime);
        Assert.Equal(new TimeOnly(20, 0), match.EndTime); // 18:30 + 90 min
    }

    [Fact]
    public void Reschedule_ToSameSlot_ReportsNoChange()
    {
        var match = CreateMatch(new DateOnly(2026, 7, 10), new TimeOnly(20, 0));
        match.Publish();

        Assert.False(match.Reschedule(new DateOnly(2026, 7, 10), new TimeOnly(20, 0)));
    }

    [Fact]
    public void Sport_DefaultsToFootball_AndCanBeSet()
    {
        var football = CreateMatch(new DateOnly(2026, 7, 10), new TimeOnly(20, 0));
        Assert.Equal(SportType.Football, football.Sport);

        var basketball = new Match(
            organizerId: Guid.NewGuid(),
            footballFieldId: Guid.NewGuid(),
            title: "Hoops",
            matchDate: new DateOnly(2026, 7, 10),
            startTime: new TimeOnly(20, 0),
            durationMinutes: 60,
            maxPlayers: 10,
            totalFieldPrice: 40m,
            reservationPaid: 0m,
            visibility: MatchVisibility.Private,
            sport: SportType.Basketball);

        Assert.Equal(SportType.Basketball, basketball.Sport);
    }
}
