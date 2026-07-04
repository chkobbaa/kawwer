using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Services;
using Kawwer.Application.Features.Push;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Tests.Application;

public sealed class WebPushTests
{
    [Fact]
    public async Task Subscribe_NewEndpoint_AddsSubscription()
    {
        var user = new User("ali", "ali@example.com", "h", "Ali", "Ben");
        var users = new FakeUserRepository(user);
        var subs = new FakeSubscriptionRepository();
        var handler = new SubscribeWebPushCommandHandler(subs, users, new FakeUnitOfWork());

        await handler.HandleAsync(
            new SubscribeWebPushCommand(user.Id, "https://push.example.com/abc", "key-a", "auth-a"),
            CancellationToken.None);

        var stored = Assert.Single(subs.Items);
        Assert.Equal("https://push.example.com/abc", stored.Endpoint);
        Assert.Equal(user.Id, stored.UserId);
    }

    [Fact]
    public async Task Subscribe_ExistingEndpoint_ReassignsAndRefreshesKeys()
    {
        var alice = new User("alice", "alice@example.com", "h", "Alice", "A");
        var bob = new User("bob", "bob@example.com", "h", "Bob", "B");
        var users = new FakeUserRepository(alice, bob);
        var subs = new FakeSubscriptionRepository();
        subs.Add(new PushSubscription(alice.Id, "https://push.example.com/shared", "old-key", "old-auth"));

        var handler = new SubscribeWebPushCommandHandler(subs, users, new FakeUnitOfWork());

        // The same browser endpoint re-subscribes after Bob signs in on that device.
        await handler.HandleAsync(
            new SubscribeWebPushCommand(bob.Id, "https://push.example.com/shared", "new-key", "new-auth"),
            CancellationToken.None);

        var stored = Assert.Single(subs.Items);
        Assert.Equal(bob.Id, stored.UserId);
        Assert.Equal("new-key", stored.P256dh);
        Assert.Equal("new-auth", stored.Auth);
    }

    [Fact]
    public async Task Notify_SendsWebPush_ToEachUserSubscription()
    {
        var user = new User("ali", "ali@example.com", "h", "Ali", "Ben");
        var users = new FakeUserRepository(user);
        var subs = new FakeSubscriptionRepository();
        subs.Add(new PushSubscription(user.Id, "https://push.example.com/1", "k1", "a1"));
        subs.Add(new PushSubscription(user.Id, "https://push.example.com/2", "k2", "a2"));
        var webPush = new FakeWebPushSender { Configured = true };
        var notifications = new FakeNotificationRepository();

        var service = new NotificationService(notifications, users, new FakePushSender(), webPush, subs);

        await service.NotifyAsync(user.Id, NotificationCategory.Invitation, "Title", "Body");

        Assert.Single(notifications.Items);
        Assert.Equal(2, webPush.SentEndpoints.Count);
        Assert.Contains("https://push.example.com/1", webPush.SentEndpoints);
        Assert.Contains("https://push.example.com/2", webPush.SentEndpoints);
    }

    [Fact]
    public async Task Notify_PrunesExpiredSubscriptions()
    {
        var user = new User("ali", "ali@example.com", "h", "Ali", "Ben");
        var users = new FakeUserRepository(user);
        var subs = new FakeSubscriptionRepository();
        subs.Add(new PushSubscription(user.Id, "https://push.example.com/dead", "k", "a"));
        var webPush = new FakeWebPushSender { Configured = true, Result = WebPushResult.Expired };

        var service = new NotificationService(new FakeNotificationRepository(), users, new FakePushSender(), webPush, subs);

        await service.NotifyAsync(user.Id, NotificationCategory.Match, "Title", "Body");

        Assert.Empty(subs.Items);
    }

    [Fact]
    public async Task Notify_SkipsWebPush_WhenNotConfigured()
    {
        var user = new User("ali", "ali@example.com", "h", "Ali", "Ben");
        var users = new FakeUserRepository(user);
        var subs = new FakeSubscriptionRepository();
        subs.Add(new PushSubscription(user.Id, "https://push.example.com/1", "k", "a"));
        var webPush = new FakeWebPushSender { Configured = false };

        var service = new NotificationService(new FakeNotificationRepository(), users, new FakePushSender(), webPush, subs);

        await service.NotifyAsync(user.Id, NotificationCategory.Match, "Title", "Body");

        Assert.Empty(webPush.SentEndpoints);
        Assert.Single(subs.Items);
    }

    // ----- Fakes -----

    private sealed class FakeSubscriptionRepository : IPushSubscriptionRepository
    {
        public List<PushSubscription> Items { get; } = new();

        public void Add(PushSubscription subscription) => Items.Add(subscription);
        public void Remove(PushSubscription subscription) => Items.Remove(subscription);

        public Task<PushSubscription?> GetByEndpointAsync(string endpoint, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(s => s.Endpoint == endpoint));

        public Task<IReadOnlyList<PushSubscription>> GetForUserAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult((IReadOnlyList<PushSubscription>)Items.Where(s => s.UserId == userId).ToList());
    }

    private sealed class FakeWebPushSender : IWebPushSender
    {
        public bool Configured { get; init; }
        public WebPushResult Result { get; init; } = WebPushResult.Delivered;
        public List<string> SentEndpoints { get; } = new();

        public bool IsConfigured => Configured;
        public string? PublicKey => Configured ? "public-key" : null;

        public Task<WebPushResult> SendAsync(
            WebPushRecipient recipient, string title, string body,
            IReadOnlyDictionary<string, string>? data = null, CancellationToken ct = default)
        {
            SentEndpoints.Add(recipient.Endpoint);
            return Task.FromResult(Result);
        }
    }

    private sealed class FakePushSender : IPushNotificationSender
    {
        public Task SendAsync(string deviceToken, string title, string body,
            IReadOnlyDictionary<string, string>? data = null, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeNotificationRepository : INotificationRepository
    {
        public List<Notification> Items { get; } = new();

        public void Add(Notification notification) => Items.Add(notification);
        public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(n => n.Id == id));
        public Task<(IReadOnlyList<Notification> Items, int Total)> GetForUserAsync(
            Guid userId, bool unreadOnly, int page, int pageSize, CancellationToken ct = default)
            => Task.FromResult(((IReadOnlyList<Notification>)Items, Items.Count));
        public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default) => Task.FromResult(0);
        public Task MarkAllReadAsync(Guid userId, CancellationToken ct = default) => Task.CompletedTask;
        public void Remove(Notification notification) => Items.Remove(notification);
        public Task RemoveForMatchAsync(Guid matchId, NotificationCategory category, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly List<User> _users;
        public FakeUserRepository(params User[] users) => _users = users.ToList();

        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
        public Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default)
            => Task.FromResult<User?>(null);
        public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default) => Task.FromResult(false);
        public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) => Task.FromResult(false);
        public Task<IReadOnlyList<User>> SearchAsync(string term, int maxResults, CancellationToken ct = default)
            => Task.FromResult((IReadOnlyList<User>)_users);
        public Task<IReadOnlyList<User>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default)
            => Task.FromResult((IReadOnlyList<User>)_users.Where(u => ids.Contains(u.Id)).ToList());
        public void Add(User user) => _users.Add(user);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(1);
    }
}
