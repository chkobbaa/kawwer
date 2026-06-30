using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Features.Auth;
using Kawwer.Domain.Entities;

namespace Kawwer.Tests.Application;

public sealed class RegisterCommandHandlerTests
{
    [Fact]
    public async Task Register_CreatesUser_AndReturnsTokens()
    {
        var users = new FakeUserRepository();
        var handler = new RegisterCommandHandler(
            users,
            new FakeRefreshTokenRepository(),
            new FakePasswordHasher(),
            new FakeJwtTokenGenerator(),
            new FakeUnitOfWork());

        var command = new RegisterCommand("ali", "ali@example.com", "Passw0rd!", "Ali", "Ben", null, null, null, null);
        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.Equal("ali", result.User.Username);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        Assert.Single(users.Items);
    }

    [Fact]
    public async Task Register_DuplicateUsername_Throws()
    {
        var users = new FakeUserRepository();
        users.Add(new User("ali", "other@example.com", "h", "Ali", "B"));

        var handler = new RegisterCommandHandler(
            users,
            new FakeRefreshTokenRepository(),
            new FakePasswordHasher(),
            new FakeJwtTokenGenerator(),
            new FakeUnitOfWork());

        var command = new RegisterCommand("ali", "new@example.com", "Passw0rd!", "Ali", "Ben", null, null, null, null);
        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<User> Items { get; } = new();

        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(u => u.Id == id));

        public Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(u =>
                string.Equals(u.Username, usernameOrEmail, StringComparison.OrdinalIgnoreCase)
                || string.Equals(u.Email, usernameOrEmail, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
            => Task.FromResult(Items.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
            => Task.FromResult(Items.Any(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)));

        public Task<IReadOnlyList<User>> SearchAsync(string term, int maxResults, CancellationToken ct = default)
            => Task.FromResult((IReadOnlyList<User>)Items);

        public Task<IReadOnlyList<User>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default)
            => Task.FromResult((IReadOnlyList<User>)Items.Where(u => ids.Contains(u.Id)).ToList());

        public void Add(User user) => Items.Add(user);
    }

    private sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
    {
        public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default) => Task.FromResult<RefreshToken?>(null);
        public Task<IReadOnlyList<RefreshToken>> GetActiveForUserAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult((IReadOnlyList<RefreshToken>)new List<RefreshToken>());
        public void Add(RefreshToken token) { }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hashed:{password}";
        public bool Verify(string password, string passwordHash) => passwordHash == $"hashed:{password}";
    }

    private sealed class FakeJwtTokenGenerator : IJwtTokenGenerator
    {
        public (string Token, DateTime ExpiresAt) GenerateAccessToken(User user) => ("access-token", DateTime.UtcNow.AddMinutes(15));
        public string GenerateRefreshToken() => "refresh-token";
        public DateTime GetRefreshTokenExpiry() => DateTime.UtcNow.AddDays(30);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(1);
    }
}
