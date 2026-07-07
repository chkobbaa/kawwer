using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Teams;

public sealed record RemoveTeamMemberCommand(Guid OwnerId, Guid TeamId, Guid MemberUserId) : IRequest<Unit>;

public sealed class RemoveTeamMemberCommandHandler : IRequestHandler<RemoveTeamMemberCommand, Unit>
{
    private readonly ITeamRepository _teams;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveTeamMemberCommandHandler(ITeamRepository teams, IUnitOfWork unitOfWork)
    {
        _teams = teams;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(RemoveTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var team = await _teams.GetByIdAsync(request.TeamId, cancellationToken)
                   ?? throw NotFoundException.For("Team", request.TeamId);

        if (team.OwnerId != request.OwnerId)
        {
            throw new ForbiddenException("Only the team owner can remove members.");
        }

        team.RemoveMember(request.MemberUserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
