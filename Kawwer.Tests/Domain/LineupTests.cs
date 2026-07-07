using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;
using Kawwer.Domain.Exceptions;
using Kawwer.Domain.Services;

namespace Kawwer.Tests.Domain;

public sealed class LineupTests
{
    private static Match CreateMatch(int maxPlayers = 14)
        => new(
            organizerId: Guid.NewGuid(),
            footballFieldId: Guid.NewGuid(),
            title: "Test Match",
            matchDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            startTime: new TimeOnly(20, 0),
            durationMinutes: 90,
            maxPlayers: maxPlayers,
            totalFieldPrice: 90m,
            reservationPaid: 5m,
            visibility: MatchVisibility.Private);

    // ----- Guest players -----

    [Fact]
    public void AddGuest_AddsNamedGuestToTheMatch()
    {
        var match = CreateMatch();
        var adder = Guid.NewGuid();

        var guest = match.AddGuest("John Doe", adder, skillLevel: 4);

        Assert.Single(match.Guests);
        Assert.Equal("John Doe", guest.Name);
        Assert.Equal(4, guest.SkillLevel);
        Assert.Equal(adder, guest.AddedByUserId);
        Assert.Equal(TeamSide.Unassigned, guest.Team);
        Assert.Null(guest.PositionX);
    }

    [Fact]
    public void AddGuest_TrimsName_AndRejectsBlank()
    {
        var match = CreateMatch();
        var guest = match.AddGuest("  John Doe  ", Guid.NewGuid());
        Assert.Equal("John Doe", guest.Name);

        Assert.Throws<DomainException>(() => match.AddGuest("   ", Guid.NewGuid()));
    }

    [Fact]
    public void AddGuest_InvalidSkillLevel_Throws()
    {
        var match = CreateMatch();
        Assert.Throws<DomainException>(() => match.AddGuest("Zed", Guid.NewGuid(), skillLevel: 9));
    }

    [Fact]
    public void AddGuest_OnClosedMatch_Throws()
    {
        var match = CreateMatch();
        match.Publish();
        match.Cancel();
        Assert.Throws<DomainException>(() => match.AddGuest("John Doe", Guid.NewGuid()));
    }

    [Fact]
    public void RemoveGuest_RemovesTheGuest()
    {
        var match = CreateMatch();
        var guest = match.AddGuest("John Doe", Guid.NewGuid());

        match.RemoveGuest(guest.Id);

        Assert.Empty(match.Guests);
        Assert.Throws<DomainException>(() => match.GetGuest(guest.Id));
    }

    // ----- Placing people on the board -----

    [Fact]
    public void PlaceGuestInLineup_ClampsCoordinatesToUnitRange()
    {
        var match = CreateMatch();
        var guest = match.AddGuest("John Doe", Guid.NewGuid());

        match.PlaceGuestInLineup(guest.Id, TeamSide.TeamB, positionX: 1.4, positionY: -0.3);

        Assert.Equal(TeamSide.TeamB, guest.Team);
        Assert.Equal(1d, guest.PositionX);
        Assert.Equal(0d, guest.PositionY);
    }

    [Fact]
    public void PlaceParticipantInLineup_RequiresAcceptedPlayer()
    {
        var match = CreateMatch();
        match.Publish();
        var invitee = Guid.NewGuid();
        match.Invite(invitee); // Invited, not accepted yet

        Assert.Throws<DomainException>(() =>
            match.PlaceParticipantInLineup(invitee, TeamSide.TeamA, 0.5, 0.5));

        match.Accept(invitee);
        match.PlaceParticipantInLineup(invitee, TeamSide.TeamA, 0.5, 0.5);
        Assert.Equal(TeamSide.TeamA, match.GetParticipant(invitee).Team);
    }

    [Fact]
    public void PlaceOrganizerInLineup_StoresSlotOnTheMatch()
    {
        var match = CreateMatch();
        match.PlaceOrganizerInLineup(TeamSide.TeamA, 0.2, 0.8);

        Assert.Equal(TeamSide.TeamA, match.OrganizerTeam);
        Assert.Equal(0.2, match.OrganizerPositionX);
        Assert.Equal(0.8, match.OrganizerPositionY);
    }

    // ----- Auto-balance algorithm -----

    [Fact]
    public void Balance_TenPlayersOfVaryingSkill_SplitsIntoTwoFairTeams()
    {
        // 10 players, skills 1..5 twice over, and a spread of reputations.
        var candidates = new List<BalanceCandidate>
        {
            new(5, 95m), new(5, 40m), new(4, 80m), new(4, 20m), new(3, 60m),
            new(3, 55m), new(2, 90m), new(2, 30m), new(1, 70m), new(1, 10m)
        };

        var placements = LineupBalancer.Balance(candidates);

        // Everyone is placed exactly once and on a real team.
        Assert.Equal(10, placements.Count);
        Assert.Equal(
            Enumerable.Range(0, 10).ToHashSet(),
            placements.Select(p => p.Index).ToHashSet());
        Assert.All(placements, p => Assert.True(p.Team is TeamSide.TeamA or TeamSide.TeamB));

        var teamA = placements.Where(p => p.Team == TeamSide.TeamA).ToList();
        var teamB = placements.Where(p => p.Team == TeamSide.TeamB).ToList();

        // Balanced squad sizes (differ by at most one) ...
        Assert.True(Math.Abs(teamA.Count - teamB.Count) <= 1);

        // ... and balanced strength: the greedy split keeps the total-score gap no larger than the
        // strongest single player (the classic longest-processing-time guarantee for two teams).
        var totalA = teamA.Sum(p => LineupBalancer.ScoreOf(candidates[p.Index]));
        var totalB = teamB.Sum(p => LineupBalancer.ScoreOf(candidates[p.Index]));
        var maxScore = candidates.Max(LineupBalancer.ScoreOf);
        Assert.True(Math.Abs(totalA - totalB) <= maxScore);

        // Positions stay on the pitch.
        Assert.All(placements, p =>
        {
            Assert.InRange(p.PositionX, 0d, 1d);
            Assert.InRange(p.PositionY, 0d, 1d);
        });
    }

    [Fact]
    public void Balance_IsDeterministic()
    {
        var candidates = new List<BalanceCandidate>
        {
            new(5, 95m), new(4, 20m), new(3, 60m), new(2, 90m), new(1, 10m)
        };

        var first = LineupBalancer.Balance(candidates);
        var second = LineupBalancer.Balance(candidates);

        Assert.Equal(
            first.Select(p => (p.Index, p.Team)),
            second.Select(p => (p.Index, p.Team)));
    }

    [Fact]
    public void Balance_SinglePlayer_GoesToTeamA()
    {
        var placements = LineupBalancer.Balance(new List<BalanceCandidate> { new(3, 50m) });
        Assert.Equal(TeamSide.TeamA, Assert.Single(placements).Team);
    }
}
