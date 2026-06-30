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
        Guid? relatedMatchId = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Category = category;
        Title = title;
        Message = message;
        RelatedMatchId = relatedMatchId;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public NotificationCategory Category { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }
    public Guid? RelatedMatchId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void MarkRead() => IsRead = true;
}
