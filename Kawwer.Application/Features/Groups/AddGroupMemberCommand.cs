using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Groups;

public sealed record AddGroupMemberCommand(Guid OwnerId, Guid GroupId, Guid MemberUserId) : IRequest<Unit>;

public sealed class AddGroupMemberCommandHandler : IRequestHandler<AddGroupMemberCommand, Unit>
{
    private readonly IGroupRepository _groups;
    private readonly IFriendshipRepository _friendships;
    private readonly IUnitOfWork _unitOfWork;

    public AddGroupMemberCommandHandler(
        IGroupRepository groups,
        IFriendshipRepository friendships,
        IUnitOfWork unitOfWork)
    {
        _groups = groups;
        _friendships = friendships;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(AddGroupMemberCommand request, CancellationToken cancellationToken)
    {
        var group = await _groups.GetByIdAsync(request.GroupId, cancellationToken)
                    ?? throw NotFoundException.For("Group", request.GroupId);

        if (group.OwnerId != request.OwnerId)
        {
            throw new ForbiddenException("Only the group owner can add members.");
        }

        // Only friends of the owner may be added to a group.
        if (!await _friendships.AreFriendsAsync(request.OwnerId, request.MemberUserId, cancellationToken))
        {
            throw new ForbiddenException("You can only add players who are already your friends.");
        }

        group.AddMember(request.MemberUserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
