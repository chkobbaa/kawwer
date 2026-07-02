using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;
using Kawwer.Domain.Exceptions;

namespace Kawwer.Tests.Domain;

public sealed class MatchTests
{
    private static Match CreateMatch(int maxPlayers = 14, decimal price = 90m, decimal reservation = 5m)
        => new(
            organizerId: Guid.NewGuid(),
            footballFieldId: Guid.NewGuid(),
            title: "Test Match",
            matchDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            startTime: new TimeOnly(20, 0),
            durationMinutes: 90,
            maxPlayers: maxPlayers,
            totalFieldPrice: price,
            reservationPaid: reservation,
            visibility: MatchVisibility.Private);

    [Fact]
    public void RemainingAmount_IsPriceMinusReservation()
    {
        var match = CreateMatch(price: 90m, reservation: 5m);
        Assert.Equal(85m, match.RemainingAmount);
    }

    [Fact]
    public void SharePerPlayer_SplitsRemainingAmongAllPlayers_RoundedUp()
    {
        // 90 TND total, 5 TND already reserved => 85 TND is the WHOLE amount left, and every
        // one of the 14 players (organizer included) pays a share => ceil(85 / 14) => 7.
        var match = CreateMatch(maxPlayers: 14, price: 90m, reservation: 5m);
        Assert.Equal(7m, match.SharePerPlayer);

        // The share is per spot, so it does not change as players accept.
        match.Publish();
        for (var i = 0; i < 13; i++)
        {
            var userId = Guid.NewGuid();
            match.Invite(userId);
            match.Accept(userId);
        }

        Assert.Equal(13, match.AcceptedCount);
        Assert.Equal(7m, match.SharePerPlayer);
    }

    [Fact]
    public void Accept_BeyondCapacity_PlacesPlayerOnWaitingList()
    {
        var match = CreateMatch(maxPlayers: 2); // one invitee spot
        match.Publish();

        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        match.Invite(first);
        match.Invite(second);

        Assert.True(match.Accept(first));   // joins
        Assert.False(match.Accept(second)); // waiting list

        Assert.Equal(1, match.AcceptedCount);
        Assert.Equal(1, match.WaitingCount);
        Assert.Equal(MatchStatus.Full, match.Status);
        Assert.Equal(1, match.GetParticipant(second).WaitingListPosition);
    }

    [Fact]
    public void Leave_PromotesFirstWaitingPlayer()
    {
        var match = CreateMatch(maxPlayers: 2);
        match.Publish();

        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        match.Invite(first);
        match.Invite(second);
        match.Accept(first);
        match.Accept(second); // waiting

        var promoted = match.Leave(first);

        Assert.NotNull(promoted);
        Assert.Equal(second, promoted!.UserId);
        Assert.Equal(ParticipantStatus.Accepted, match.GetParticipant(second).Status);
        Assert.Equal(0, match.WaitingCount);
        Assert.Equal(MatchStatus.Full, match.Status);
    }

    [Fact]
    public void Invite_Duplicate_Throws()
    {
        var match = CreateMatch();
        var userId = Guid.NewGuid();
        match.Invite(userId);
        Assert.Throws<DomainException>(() => match.Invite(userId));
    }

    [Fact]
    public void Invite_Organizer_Throws()
    {
        var match = CreateMatch();
        Assert.Throws<DomainException>(() => match.Invite(match.OrganizerId));
    }

    [Fact]
    public void ChangeMaxPlayers_BelowAccepted_Throws()
    {
        var match = CreateMatch(maxPlayers: 4);
        match.Publish();
        for (var i = 0; i < 3; i++)
        {
            var u = Guid.NewGuid();
            match.Invite(u);
            match.Accept(u);
        }

        // 3 accepted; capacity must stay >= 4 (3 players + organizer).
        Assert.Throws<DomainException>(() => match.ChangeMaxPlayers(3));
    }

    [Fact]
    public void IncreasingCapacity_PromotesWaitingPlayers()
    {
        var match = CreateMatch(maxPlayers: 2);
        match.Publish();

        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        match.Invite(a);
        match.Invite(b);
        match.Accept(a);
        match.Accept(b); // waiting

        match.ChangeMaxPlayers(3); // now 2 invitee spots

        Assert.Equal(2, match.AcceptedCount);
        Assert.Equal(0, match.WaitingCount);
    }

    [Fact]
    public void FinishPaymentCollection_RequiresZeroMissing()
    {
        var match = CreateMatch(maxPlayers: 2, price: 50m, reservation: 0m);
        match.Publish();
        var u = Guid.NewGuid();
        match.Invite(u);
        match.Accept(u);
        match.StartPaymentCollection();

        Assert.Throws<DomainException>(() => match.FinishPaymentCollection());

        match.RecordPayment(u, 50m);
        match.FinishPaymentCollection();
        Assert.True(match.PaymentCompleted);
    }

    [Fact]
    public void Cancel_FinishedMatch_Throws()
    {
        var match = CreateMatch();
        match.Publish();
        match.Finish();
        Assert.Throws<DomainException>(() => match.Cancel());
    }
}
