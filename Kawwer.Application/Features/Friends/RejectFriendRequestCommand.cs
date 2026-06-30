using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Friends;

public sealed record RejectFriendRequestCommand(Guid UserId, Guid FriendshipId) : IRequest<Unit>;

public sealed class RejectFriendRequestCommandHandler : IRequestHandler<RejectFriendRequestCommand, Unit>
{
    private readonly IFriendshipRepository _friendships;
    private readonly IUnitOfWork _unitOfWork;

    public RejectFriendRequestCommandHandler(IFriendshipRepository friendships, IUnitOfWork unitOfWork)
    {
        _friendships = friendships;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(RejectFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendship = await _friendships.GetByIdAsync(request.FriendshipId, cancellationToken)
                         ?? throw NotFoundException.For("Friend request", request.FriendshipId);

        // Either party may dismiss a pending request.
        if (!friendship.Involves(request.UserId))
        {
            throw new ForbiddenException("You cannot reject this friend request.");
        }

        _friendships.Remove(friendship);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
