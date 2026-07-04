using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Features.Matches;
using Kawwer.Application.Features.Users;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Tests.Application;

public sealed class AccountAndParticipantsTests
{
    [Fact]
    public async Task DeleteAccount_SoftDeletesUser_AndRevokesTokens()
    {
        var user = new User("ali", "ali@example.com", "h", "Ali", "Ben");
        var users = new FakeUserRepository();
        users.Add(user);

        var tokens = new FakeRefreshTokenRepository();
        var active = new RefreshToken(user.Id, "token", DateTime.UtcNow.AddDays(1));
        tokens.Items.Add(active);

        var uow = new FakeUnitOfWork();
        var handler = new DeleteAccountCommandHandler(users, tokens, uow);

        await handler.HandleAsync(new DeleteAccountCommand(user.Id), CancellationToken.None);

        Assert.False(user.IsActive);
        Assert.Equal(AccountStatus.Deleted, user.Status);
        Assert.False(active.IsActive);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task GetMatchParticipants_IncludesOrganizerAsAcceptedPlayer()
    {
        var organizer = new User("org", "org@example.com", "h", "Org", "Anizer");
        var invitee = new User("inv", "inv@example.com", "h", "In", "Vitee");

        var match = new Match(
            organizerId: organizer.Id,
            footballFieldId: Guid.NewGuid(),
            title: "Test Match",
            matchDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            startTime: new TimeOnly(20, 0),
            durationMinutes: 90,
            maxPlayers: 14,
            totalFieldPrice: 90m,
            reservationPaid: 5m,
            visibility: MatchVisibility.Public);
        match.Publish();
        match.Invite(invitee.Id);
        match.Accept(invitee.Id);

        var users = new FakeUserRepository();
        users.Add(organizer);
        users.Add(invitee);

        var handler = new GetMatchParticipantsQueryHandler(new FakeMatchRepository(match), users);
        var result = await handler.HandleAsync(new GetMatchParticipantsQuery(match.Id), CancellationToken.None);

        // The organizer heads the list, is marked Accepted, and is never duplicated.
        Assert.Equal(organizer.Id, result[0].User.Id);
        Assert.Equal(ParticipantStatus.Accepted, result[0].Status);
        Assert.Equal(Guid.Empty, result[0].Id);
        Assert.Single(result, r => r.User.Id == organizer.Id);
        Assert.Contains(result, r => r.User.Id == invitee.Id);
    }

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

    private sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
    {
        public List<RefreshToken> Items { get; } = new();

        public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(t => t.Token == token));
        public Task<IReadOnlyList<RefreshToken>> GetActiveForUserAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult((IReadOnlyList<RefreshToken>)Items.Where(t => t.UserId == userId && t.IsActive).ToList());
        public void Add(RefreshToken token) => Items.Add(token);
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
}
