using FluentValidation;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Lineup;

/// <summary>
/// Moves one slot on the tactical board to a team and a normalized position. Only the organizer can
/// arrange the board (it is the coach's board). Coordinates are 0..1 within the slot's own team half.
/// </summary>
public sealed record UpdateLineupSlotCommand(
    Guid RequesterId,
    Guid MatchId,
    LineupSlotKind Kind,
    Guid TargetId,
    TeamSide Team,
    double PositionX,
    double PositionY) : IRequest<Unit>;

public sealed class UpdateLineupSlotCommandValidator : AbstractValidator<UpdateLineupSlotCommand>
{
    public UpdateLineupSlotCommandValidator()
    {
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Team).IsInEnum();
        RuleFor(x => x.PositionX).InclusiveBetween(0d, 1d);
        RuleFor(x => x.PositionY).InclusiveBetween(0d, 1d);
    }
}

public sealed class UpdateLineupSlotCommandHandler : IRequestHandler<UpdateLineupSlotCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLineupSlotCommandHandler(
        IMatchRepository matches,
        IRealtimeNotifier realtime,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _realtime = realtime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(UpdateLineupSlotCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.RequesterId)
        {
            throw new ForbiddenException("Only the organizer can arrange the lineup board.");
        }

        switch (request.Kind)
        {
            case LineupSlotKind.Organizer:
                match.PlaceOrganizerInLineup(request.Team, request.PositionX, request.PositionY);
                break;
            case LineupSlotKind.Participant:
                match.PlaceParticipantInLineup(request.TargetId, request.Team, request.PositionX, request.PositionY);
                break;
            case LineupSlotKind.Guest:
                match.PlaceGuestInLineup(request.TargetId, request.Team, request.PositionX, request.PositionY);
                break;
            default:
                throw new Common.Exceptions.ValidationException("Unknown lineup slot kind.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.MatchUpdatedAsync(match.Id, cancellationToken);
        return Unit.Value;
    }
}
