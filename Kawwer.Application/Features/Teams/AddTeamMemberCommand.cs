using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Teams;

public sealed record AddTeamMemberCommand(Guid OwnerId, Guid TeamId, Guid MemberUserId) : IRequest<Unit>;

public sealed class AddTeamMemberCommandHandler : IRequestHandler<AddTeamMemberCommand, Unit>
{
    private readonly ITeamRepository _teams;
    private readonly IFriendshipRepository _friendships;
    private readonly IUnitOfWork _unitOfWork;

    public AddTeamMemberCommandHandler(
        ITeamRepository teams,
        IFriendshipRepository friendships,
        IUnitOfWork unitOfWork)
    {
        _teams = teams;
        _friendships = friendships;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(AddTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var team = await _teams.GetByIdAsync(request.TeamId, cancellationToken)
                   ?? throw NotFoundException.For("Team", request.TeamId);

        if (team.OwnerId != request.OwnerId)
        {
            throw new ForbiddenException("Only the team owner can add members.");
        }

        // Only friends of the owner may be added to a team.
        if (!await _friendships.AreFriendsAsync(request.OwnerId, request.MemberUserId, cancellationToken))
        {
            throw new ForbiddenException("You can only add players who are already your friends.");
        }

        team.AddMember(request.MemberUserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
