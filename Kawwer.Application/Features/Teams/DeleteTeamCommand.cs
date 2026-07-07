using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Teams;

public sealed record DeleteTeamCommand(Guid OwnerId, Guid TeamId) : IRequest<Unit>;

public sealed class DeleteTeamCommandHandler : IRequestHandler<DeleteTeamCommand, Unit>
{
    private readonly ITeamRepository _teams;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTeamCommandHandler(ITeamRepository teams, IUnitOfWork unitOfWork)
    {
        _teams = teams;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(DeleteTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await _teams.GetByIdAsync(request.TeamId, cancellationToken)
                   ?? throw NotFoundException.For("Team", request.TeamId);

        if (team.OwnerId != request.OwnerId)
        {
            throw new ForbiddenException("Only the team owner can delete this team.");
        }

        _teams.Remove(team);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
