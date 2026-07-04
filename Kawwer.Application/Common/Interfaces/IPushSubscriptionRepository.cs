using Kawwer.Domain.Entities;

namespace Kawwer.Application.Common.Interfaces;

/// <summary>Persistence for Web Push subscriptions (the PWA counterpart of the FCM device token).</summary>
public interface IPushSubscriptionRepository
{
    void Add(PushSubscription subscription);
    void Remove(PushSubscription subscription);

    /// <summary>Finds a subscription by its (unique) push-service endpoint, if any.</summary>
    Task<PushSubscription?> GetByEndpointAsync(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>All active subscriptions for a user; a user may have installed the PWA on several devices.</summary>
    Task<IReadOnlyList<PushSubscription>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
