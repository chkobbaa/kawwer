namespace Kawwer.Mobile.Models;

/// <summary>
/// Client mirror of the API's user-scoped real-time signal (Kawwer.Contracts.Realtime). The
/// property names line up with the SignalR payload (and the Firebase data payload), so the app
/// can react to a friend request, invitation, match change or profile change the instant it
/// happens — then re-fetch through the normal API. The mobile app doesn't reference the shared
/// Contracts assembly, so this small record is duplicated here on purpose.
///
/// <see cref="Important"/> flags high-priority signals the app may escalate (a simulated incoming
/// call in "Call" mode); <see cref="Title"/>/<see cref="Message"/> let that escalation render
/// without an extra round trip.
/// </summary>
public sealed record RealtimeUserEvent(
    string Category,
    string? Type = null,
    Guid? MatchId = null,
    string? FriendshipId = null,
    bool Important = false,
    string? Title = null,
    string? Message = null);
