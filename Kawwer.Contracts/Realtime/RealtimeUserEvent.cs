namespace Kawwer.Contracts.Realtime;

/// <summary>
/// A lightweight, user-scoped real-time signal pushed over SignalR to every connection a single
/// user currently has open. It tells the client "something you care about changed" so it can
/// refresh the affected screen instantly — without polling.
///
/// It intentionally carries only identifiers plus a short human-readable title/message, never full
/// entities: the client re-fetches through the normal REST API, so the same authorization and
/// mapping rules always apply. The field names mirror the Firebase data payload the mobile app
/// already understands (category/type/matchId/friendshipId), so the client can reuse a single
/// mapping for both channels.
///
/// <paramref name="Important"/> escalates delivery on the client (e.g. a simulated incoming call in
/// "Call" mode); <paramref name="Title"/>/<paramref name="Message"/> let that escalation render
/// instantly without an extra round trip.
/// </summary>
public sealed record RealtimeUserEvent(
    string Category,
    string? Type = null,
    Guid? MatchId = null,
    string? FriendshipId = null,
    bool Important = false,
    string? Title = null,
    string? Message = null);
