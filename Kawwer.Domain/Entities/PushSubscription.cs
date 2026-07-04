using Kawwer.Domain.Common;

namespace Kawwer.Domain.Entities;

/// <summary>
/// A Web Push subscription for a single browser/PWA install (W3C Push API). Created when a user
/// enables notifications in the web app. A user may hold several — one per device/browser — and
/// each is pruned when the push service reports it as expired (HTTP 404/410).
/// </summary>
/// <remarks>
/// This is the web counterpart to <see cref="User.DeviceToken"/> (FCM, native mobile). The three
/// fields together are exactly the output of the browser's <c>PushSubscription.toJSON()</c>: the
/// push service <see cref="Endpoint"/> plus the two client keys (<see cref="P256dh"/> and
/// <see cref="Auth"/>) that encrypt the payload end-to-end.
/// </remarks>
public class PushSubscription : AggregateRoot
{
    // Parameterless constructor for EF Core materialization.
    private PushSubscription()
    {
        Endpoint = string.Empty;
        P256dh = string.Empty;
        Auth = string.Empty;
    }

    public PushSubscription(Guid userId, string endpoint, string p256dh, string auth)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Endpoint = endpoint;
        P256dh = p256dh;
        Auth = auth;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }

    /// <summary>The push service URL the browser gave us; unique per subscription.</summary>
    public string Endpoint { get; private set; }

    /// <summary>The client's public P-256 ECDH key (base64url), used to encrypt the payload.</summary>
    public string P256dh { get; private set; }

    /// <summary>The client's auth secret (base64url), used to encrypt the payload.</summary>
    public string Auth { get; private set; }

    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Re-points an existing endpoint at a user and refreshes its keys. Used when the same browser
    /// re-subscribes — because a browser reuses one endpoint per push service, whether the keys
    /// rotated or a different account signed in on that device.
    /// </summary>
    public void AssignTo(Guid userId, string p256dh, string auth)
    {
        UserId = userId;
        P256dh = p256dh;
        Auth = auth;
    }
}
