using FluentValidation;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Matches;

public sealed record CreateMatchCommand(
    Guid OrganizerId,
    Guid FootballFieldId,
    string? Title,
    string? Description,
    DateOnly MatchDate,
    TimeOnly StartTime,
    int? MaxPlayers,
    decimal? TotalFieldPrice,
    MatchVisibility Visibility,
    bool AutoAcceptPublic,
    IReadOnlyList<Guid> InvitedUserIds,
    IReadOnlyList<Guid> InvitedGroupIds) : IRequest<Guid>;

public sealed class CreateMatchCommandValidator : AbstractValidator<CreateMatchCommand>
{
    public CreateMatchCommandValidator()
    {
        RuleFor(x => x.FootballFieldId).NotEmpty();
        RuleFor(x => x.MatchDate).NotEmpty();
        RuleFor(x => x.MaxPlayers).GreaterThanOrEqualTo(2).When(x => x.MaxPlayers.HasValue);
        RuleFor(x => x.TotalFieldPrice).GreaterThanOrEqualTo(0).When(x => x.TotalFieldPrice.HasValue);
        RuleFor(x => x)
            .Must(x => x.Visibility != MatchVisibility.Private
                       || x.InvitedUserIds.Count > 0
                       || x.InvitedGroupIds.Count > 0)
            .WithMessage("An invitations-only match must invite at least one player or group.");
    }
}

public sealed class CreateMatchCommandHandler : IRequestHandler<CreateMatchCommand, Guid>
{
    private readonly IMatchRepository _matches;
    private readonly IFootballFieldRepository _fields;
    private readonly IGroupRepository _groups;
    private readonly IChatRepository _chat;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMatchCommandHandler(
        IMatchRepository matches,
        IFootballFieldRepository fields,
        IGroupRepository groups,
        IChatRepository chat,
        INotificationService notifications,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _fields = fields;
        _groups = groups;
        _chat = chat;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(CreateMatchCommand request, CancellationToken cancellationToken)
    {
        var field = await _fields.GetByIdAsync(request.FootballFieldId, cancellationToken)
                    ?? throw NotFoundException.For("Football field", request.FootballFieldId);

        var maxPlayers = request.MaxPlayers ?? field.Capacity;
        var totalPrice = request.TotalFieldPrice ?? field.Price;

        var match = new Match(
            request.OrganizerId,
            field.Id,
            string.IsNullOrWhiteSpace(request.Title) ? "Football Match" : request.Title!.Trim(),
            request.MatchDate,
            request.StartTime,
            field.MatchDurationMinutes,
            maxPlayers,
            totalPrice,
            field.ReservationFee,
            request.Visibility,
            request.Description,
            request.AutoAcceptPublic);

        // Resolve the set of invitees: explicit users plus members of the selected groups.
        var inviteeIds = new HashSet<Guid>(request.InvitedUserIds);
        foreach (var groupId in request.InvitedGroupIds)
        {
            var group = await _groups.GetByIdAsync(groupId, cancellationToken);
            if (group is null || group.OwnerId != request.OrganizerId)
            {
                continue;
            }

            foreach (var member in group.Members)
            {
                inviteeIds.Add(member.UserId);
            }
        }

        inviteeIds.Remove(request.OrganizerId);

        foreach (var userId in inviteeIds)
        {
            match.Invite(userId);
        }

        match.Publish();
        _matches.Add(match);

        _chat.Add(ChatMessage.CreateSystemMessage(match.Id, "The match chat is now open. Coordinate here!"));

        await _notifications.NotifyManyAsync(
            inviteeIds,
            NotificationCategory.Invitation,
            "New match invitation",
            $"You've been invited to \"{match.Title}\" on {match.MatchDate:dd MMM} at {match.StartTime:HH\\:mm}.",
            match.Id,
            cancellationToken,
            // Tells the mobile app to render Accept/Decline action buttons on the push.
            new Dictionary<string, string> { ["type"] = "match_invitation" });

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return match.Id;
    }
}
