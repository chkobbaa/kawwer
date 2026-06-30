using Kawwer.Domain.Common;
using Kawwer.Domain.Enums;
using Kawwer.Domain.Exceptions;

namespace Kawwer.Domain.Entities;

/// <summary>
/// A message in a match's temporary chat room. Supports user and system messages.
/// Users may edit or delete their own messages within a short window.
/// </summary>
public class ChatMessage : Entity
{
    private static readonly TimeSpan EditWindow = TimeSpan.FromMinutes(5);

    private ChatMessage()
    {
        Content = string.Empty;
    }

    private ChatMessage(Guid matchId, Guid? senderId, ChatMessageType type, string content)
    {
        Id = Guid.NewGuid();
        MatchId = matchId;
        SenderId = senderId;
        Type = type;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid MatchId { get; private set; }

    /// <summary>Null for system messages.</summary>
    public Guid? SenderId { get; private set; }
    public ChatMessageType Type { get; private set; }
    public string Content { get; private set; }
    public bool IsDeleted { get; private set; }
    public bool IsEdited { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? EditedAt { get; private set; }

    public static ChatMessage CreateUserMessage(Guid matchId, Guid senderId, string content)
        => new(matchId, senderId, ChatMessageType.User, content);

    public static ChatMessage CreateSystemMessage(Guid matchId, string content)
        => new(matchId, null, ChatMessageType.System, content);

    public void Edit(Guid editorId, string content)
    {
        EnsureUserMessage();
        if (SenderId != editorId)
        {
            throw new DomainException("Only the author can edit a message.");
        }

        if (DateTime.UtcNow - CreatedAt > EditWindow)
        {
            throw new DomainException("Messages can only be edited within five minutes.");
        }

        Content = content;
        IsEdited = true;
        EditedAt = DateTime.UtcNow;
    }

    public void Delete(Guid editorId)
    {
        EnsureUserMessage();
        if (SenderId != editorId)
        {
            throw new DomainException("Only the author can delete a message.");
        }

        if (DateTime.UtcNow - CreatedAt > EditWindow)
        {
            throw new DomainException("Messages can only be deleted within five minutes.");
        }

        IsDeleted = true;
        Content = string.Empty;
    }

    private void EnsureUserMessage()
    {
        if (Type != ChatMessageType.User)
        {
            throw new DomainException("System messages cannot be modified.");
        }
    }
}
