using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.LiveMatch;

/// <summary>
/// Updates a participant's attendance. The organizer may set it for anyone; a player may only
/// mark themselves (typically "I've arrived").
/// </summary>
public sealed record UpdateAttendanceCommand(
    Guid CallerId,
    Guid MatchId,
    Guid TargetUserId,
    AttendanceStatus Attendance) : IRequest<Unit>;

public sealed class UpdateAttendanceCommandHandler : IRequestHandler<UpdateAttendanceCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAttendanceCommandHandler(
        IMatchRepository matches,
        IRealtimeNotifier realtime,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _realtime = realtime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(UpdateAttendanceCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        var isOrganizer = match.OrganizerId == request.CallerId;
        if (!isOrganizer && request.CallerId != request.TargetUserId)
        {
            throw new ForbiddenException("You can only update your own attendance.");
        }

        var participant = match.GetParticipant(request.TargetUserId);
        participant.SetAttendance(request.Attendance);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.MatchUpdatedAsync(match.Id, cancellationToken);
        return Unit.Value;
    }
}
