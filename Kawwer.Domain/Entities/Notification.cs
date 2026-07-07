using Kawwer.Domain.Common;
using Kawwer.Domain.Enums;

namespace Kawwer.Domain.Entities;

/// <summary>
/// A persistent in-app notification. Every push notification also creates one of these (D-016).
/// </summary>
public class Notification : Entity
{
    private Notification()
    {
        Title = string.Empty;
        Message = string.Empty;
    }

    public Notification(
        Guid userId,
        NotificationCategory category,
        string title,
        string message,
        Guid? relatedMatchId = null,
        string? type = null,
        Guid? relatedFriendshipId = null,
        bool important = false)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Category = category;
        Title = title;
        Message = message;
        RelatedMatchId = relatedMatchId;
        Type = type;
        RelatedFriendshipId = relatedFriendshipId;
        Important = important;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public NotificationCategory Category { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }
    public Guid? RelatedMatchId { get; private set; }

    /// <summary>
    /// A stable machine-readable kind (e.g. "friend_request", "match_invitation",
    /// "match_rescheduled"). Lets the client decide which inline actions to render without brittle
    /// title string matching.
    /// </summary>
    public string? Type { get; private set; }

    /// <summary>The friendship this notification acts on, for friend-request Accept/Decline actions.</summary>
    public Guid? RelatedFriendshipId { get; private set; }

    /// <summary>
    /// Marks a high-priority notification (reschedules, cancellations). The client may escalate
    /// these — e.g. simulate an incoming call in "Call" mode — instead of a silent notification.
    /// </summary>
    public bool Important { get; private set; }

    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void MarkRead() => IsRead = true;
}
