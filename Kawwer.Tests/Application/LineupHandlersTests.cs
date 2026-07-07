using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Features.Lineup;
using Kawwer.Contracts.Chat;
using Kawwer.Contracts.Realtime;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Tests.Application;

public sealed class LineupHandlersTests
{
    private static Match CreateMatch(Guid organizerId, int maxPlayers = 14)
    {
        var match = new Match(
            organizerId: organizerId,
            footballFieldId: Guid.NewGuid(),
            title: "Sunday Kickabout",
            matchDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            startTime: new TimeOnly(20, 0),
            durationMinutes: 90,
            maxPlayers: maxPlayers,
            totalFieldPrice: 90m,
            reservationPaid: 5m,
            visibility: MatchVisibility.Private);
        match.Publish();
        return match;
    }

    [Fact]
    public async Task AddGuest_AddsNamedGuest_AndReturnsIt()
    {
        var organizer = new User("org", "org@example.com", "h", "Org", "Anizer");
        var match = CreateMatch(organizer.Id);

        var users = new FakeUserRepository();
        users.Add(organizer);
        var uow = new FakeUnitOfWork();
        var handler = new AddGuestPlayerCommandHandler(new FakeMatchRepository(match), new FakeRealtimeNotifier(), uow);

        var dto = await handler.HandleAsync(
            new AddGuestPlayerCommand(organizer.Id, match.Id, "John Doe", SkillLevel: 3),
            CancellationToken.None);

        Assert.Equal("John Doe", dto.Name);
        Assert.Equal(3, dto.SkillLevel);
        Assert.Single(match.Guests);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task AddGuest_ByStranger_IsForbidden()
    {
        var organizer = new User("org", "org@example.com", "h", "Org", "Anizer");
        var match = CreateMatch(organizer.Id);
        var handler = new AddGuestPlayerCommandHandler(new FakeMatchRepository(match), new FakeRealtimeNotifier(), new FakeUnitOfWork());

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.HandleAsync(
            new AddGuestPlayerCommand(Guid.NewGuid(), match.Id, "John Doe", null), CancellationToken.None));
    }

    [Fact]
    public async Task AutoBalance_AssignsEveryoneToATeam()
    {
        var organizer = new User("org", "org@example.com", "h", "Org", "Anizer");
        var match = CreateMatch(organizer.Id);

        var users = new FakeUserRepository();
        users.Add(organizer);

        // Five accepted players of varying skill.
        var skills = new int?[] { 5, 4, 3, 2, 1 };
        foreach (var skill in skills)
        {
            var user = new User("p" + skill, $"p{skill}@example.com", "h", "Player", skill.ToString()!);
            user.UpdateProfile("Player", skill.ToString()!, null, null, null, null, skill, ProfileVisibility.Public);
            users.Add(user);
            match.Invite(user.Id);
            match.Accept(user.Id);
        }

        // Two guests of different skill.
        match.AddGuest("Guest One", organizer.Id, skillLevel: 5);
        match.AddGuest("Guest Two", organizer.Id, skillLevel: 1);

        var handler = new AutoBalanceLineupCommandHandler(
            new FakeMatchRepository(match), users, new FakeRealtimeNotifier(), new FakeUnitOfWork());

        var lineup = await handler.HandleAsync(new AutoBalanceLineupCommand(organizer.Id, match.Id), CancellationToken.None);

        // organizer + 5 players + 2 guests = 8 slots, all on a real team.
        Assert.Equal(8, lineup.Slots.Count);
        Assert.All(lineup.Slots, s => Assert.True(s.Team is TeamSide.TeamA or TeamSide.TeamB));
        Assert.All(lineup.Slots, s =>
        {
            Assert.NotNull(s.PositionX);
            Assert.NotNull(s.PositionY);
        });

        var teamA = lineup.Slots.Count(s => s.Team == TeamSide.TeamA);
        var teamB = lineup.Slots.Count(s => s.Team == TeamSide.TeamB);
        Assert.True(Math.Abs(teamA - teamB) <= 1);
    }

    [Fact]
    public async Task AutoBalance_ByNonOrganizer_IsForbidden()
    {
        var organizer = new User("org", "org@example.com", "h", "Org", "Anizer");
        var match = CreateMatch(organizer.Id);
        var users = new FakeUserRepository();
        users.Add(organizer);
        var handler = new AutoBalanceLineupCommandHandler(
            new FakeMatchRepository(match), users, new FakeRealtimeNotifier(), new FakeUnitOfWork());

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.HandleAsync(
            new AutoBalanceLineupCommand(Guid.NewGuid(), match.Id), CancellationToken.None));
    }

    [Fact]
    public async Task GetLineup_IncludesOrganizerAcceptedPlayersAndGuests()
    {
        var organizer = new User("org", "org@example.com", "h", "Org", "Anizer");
        var player = new User("ply", "ply@example.com", "h", "Reg", "Ular");
        var match = CreateMatch(organizer.Id);
        match.Invite(player.Id);
        match.Accept(player.Id);
        match.AddGuest("John Doe", organizer.Id);

        var users = new FakeUserRepository();
        users.Add(organizer);
        users.Add(player);

        var handler = new GetLineupQueryHandler(new FakeMatchRepository(match), users);
        var lineup = await handler.HandleAsync(new GetLineupQuery(organizer.Id, match.Id), CancellationToken.None);

        Assert.Contains(lineup.Slots, s => s.Kind == LineupSlotKind.Organizer && s.Id == organizer.Id);
        Assert.Contains(lineup.Slots, s => s.Kind == LineupSlotKind.Participant && s.Id == player.Id);
        Assert.Contains(lineup.Slots, s => s.Kind == LineupSlotKind.Guest && s.DisplayName == "John Doe");
    }

    [Fact]
    public async Task UpdateLineupSlot_ByNonOrganizer_IsForbidden()
    {
        var organizer = new User("org", "org@example.com", "h", "Org", "Anizer");
        var match = CreateMatch(organizer.Id);
        var handler = new UpdateLineupSlotCommandHandler(
            new FakeMatchRepository(match), new FakeRealtimeNotifier(), new FakeUnitOfWork());

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.HandleAsync(
            new UpdateLineupSlotCommand(Guid.NewGuid(), match.Id, LineupSlotKind.Organizer, organizer.Id, TeamSide.TeamA, 0.5, 0.5),
            CancellationToken.None));
    }

    // ----- Fakes -----

    private sealed class FakeMatchRepository : IMatchRepository
    {
        private readonly Match _match;
        public FakeMatchRepository(Match match) => _match = match;

        public Task<Match?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_match.Id == id ? _match : null);

        public Task<IReadOnlyList<Match>> GetForOrganizerAsync(Guid organizerId, CancellationToken ct = default)
            => throw new NotImplementedException();
        public Task<IReadOnlyList<Match>> GetUpcomingForUserAsync(Guid userId, CancellationToken ct = default)
            => throw new NotImplementedException();
        public Task<IReadOnlyList<Match>> GetForUserParticipationAsync(Guid userId, CancellationToken ct = default)
            => throw new NotImplementedException();
        public Task<(IReadOnlyList<Match> Items, int Total)> GetPublicAsync(
            DateOnly? dateFrom, DateOnly? dateTo, IReadOnlyCollection<Guid> friendOrganizerIds,
            int page, int pageSize, CancellationToken ct = default)
            => throw new NotImplementedException();
        public void Add(Match match) => throw new NotImplementedException();
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<User> Items { get; } = new();

        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(u => u.Id == id));
        public Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default)
            => Task.FromResult<User?>(null);
        public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default) => Task.FromResult(false);
        public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) => Task.FromResult(false);
        public Task<IReadOnlyList<User>> SearchAsync(string term, int maxResults, CancellationToken ct = default)
            => Task.FromResult((IReadOnlyList<User>)Items);
        public Task<IReadOnlyList<User>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default)
            => Task.FromResult((IReadOnlyList<User>)Items.Where(u => ids.Contains(u.Id)).ToList());
        public void Add(User user) => Items.Add(user);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }
        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class FakeRealtimeNotifier : IRealtimeNotifier
    {
        public Task ChatMessagePostedAsync(Guid matchId, ChatMessageDto message, CancellationToken ct = default) => Task.CompletedTask;
        public Task MatchUpdatedAsync(Guid matchId, CancellationToken ct = default) => Task.CompletedTask;
        public Task PaymentUpdatedAsync(Guid matchId, CancellationToken ct = default) => Task.CompletedTask;
        public Task WaitingListUpdatedAsync(Guid matchId, CancellationToken ct = default) => Task.CompletedTask;
        public Task NotifyUserAsync(Guid userId, RealtimeUserEvent @event, CancellationToken ct = default) => Task.CompletedTask;
    }
}
