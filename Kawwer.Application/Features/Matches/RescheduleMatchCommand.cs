using FluentValidation;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Matches;

/// <summary>
/// The organizer moves a match to a new date/time. Everyone still attached to the match — accepted
/// players, the waiting list, and anyone with an open/undecided invitation — is notified. The
/// notification is flagged <c>Important</c> and typed <c>match_rescheduled</c> so clients in "Call"
/// mode can escalate it (a schedule change is easy to miss with a silent push).
/// </summary>
public sealed record RescheduleMatchCommand(
    Guid OrganizerId,
    Guid MatchId,
    DateOnly MatchDate,
    TimeOnly StartTime) : IRequest<Unit>;

public sealed class RescheduleMatchCommandValidator : AbstractValidator<RescheduleMatchCommand>
{
    public RescheduleMatchCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.MatchDate).NotEmpty();
    }
}

public sealed class RescheduleMatchCommandHandler : IRequestHandler<RescheduleMatchCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IChatRepository _chat;
    private readonly INotificationService _notifications;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public RescheduleMatchCommandHandler(
        IMatchRepository matches,
        IChatRepository chat,
        INotificationService notifications,
        IRealtimeNotifier realtime,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _chat = chat;
        _notifications = notifications;
        _realtime = realtime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(RescheduleMatchCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.OrganizerId)
        {
            throw new ForbiddenException("Only the organizer can reschedule this match.");
        }

        var previous = $"{match.MatchDate:dd MMM} at {match.StartTime:HH\\:mm}";
        if (!match.Reschedule(request.MatchDate, request.StartTime))
        {
            // No-op edit: don't spam the roster with a "rescheduled" alert.
            return Unit.Value;
        }

        _chat.Add(ChatMessage.CreateSystemMessage(
            match.Id,
            $"The match was moved from {previous} to {match.MatchDate:dd MMM} at {match.StartTime:HH\\:mm}."));

        // Everyone with a live relationship to the match: confirmed players, the waiting list, and
        // anyone still holding an undecided invitation.
        var affected = match.Participants
            .Where(p => p.Status is ParticipantStatus.Accepted
                        or ParticipantStatus.WaitingList
                        or ParticipantStatus.Invited
                        or ParticipantStatus.Seen
                        or ParticipantStatus.Thinking)
            .Select(p => p.UserId);

        await _notifications.NotifyManyAsync(
            affected,
            NotificationCategory.Match,
            "Match rescheduled",
            $"\"{match.Title}\" moved to {match.MatchDate:dd MMM} at {match.StartTime:HH\\:mm}.",
            match.Id,
            cancellationToken,
            type: "match_rescheduled",
            important: true);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.MatchUpdatedAsync(match.Id, cancellationToken);
        return Unit.Value;
    }
}
