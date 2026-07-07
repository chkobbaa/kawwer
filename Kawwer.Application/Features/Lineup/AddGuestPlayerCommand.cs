using FluentValidation;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Matches;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Lineup;

/// <summary>
/// Adds a guest player (someone without the app) to a match. The organizer or any accepted player
/// can add a guest, mirroring how members may suggest real players.
/// </summary>
public sealed record AddGuestPlayerCommand(
    Guid RequesterId,
    Guid MatchId,
    string Name,
    int? SkillLevel) : IRequest<GuestPlayerDto>;

public sealed class AddGuestPlayerCommandValidator : AbstractValidator<AddGuestPlayerCommand>
{
    public AddGuestPlayerCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(60);
        RuleFor(x => x.SkillLevel!.Value).InclusiveBetween(1, 5).When(x => x.SkillLevel.HasValue);
    }
}

public sealed class AddGuestPlayerCommandHandler : IRequestHandler<AddGuestPlayerCommand, GuestPlayerDto>
{
    private readonly IMatchRepository _matches;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public AddGuestPlayerCommandHandler(
        IMatchRepository matches,
        IRealtimeNotifier realtime,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _realtime = realtime;
        _unitOfWork = unitOfWork;
    }

    public async Task<GuestPlayerDto> HandleAsync(AddGuestPlayerCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        var isOrganizer = match.OrganizerId == request.RequesterId;
        var isMember = match.Participants.Any(p => p.UserId == request.RequesterId
                                                   && p.Status == ParticipantStatus.Accepted);
        if (!isOrganizer && !isMember)
        {
            throw new ForbiddenException("Only the organizer or a player in the match can add a guest.");
        }

        var guest = match.AddGuest(request.Name, request.RequesterId, request.SkillLevel);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.MatchUpdatedAsync(match.Id, cancellationToken);
        return guest.ToDto();
    }
}
