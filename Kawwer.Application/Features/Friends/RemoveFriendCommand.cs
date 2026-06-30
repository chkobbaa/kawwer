using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Friends;

public sealed record RemoveFriendCommand(Guid UserId, Guid FriendUserId) : IRequest<Unit>;

public sealed class RemoveFriendCommandHandler : IRequestHandler<RemoveFriendCommand, Unit>
{
    private readonly IFriendshipRepository _friendships;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveFriendCommandHandler(IFriendshipRepository friendships, IUnitOfWork unitOfWork)
    {
        _friendships = friendships;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(RemoveFriendCommand request, CancellationToken cancellationToken)
    {
        var friendship = await _friendships.GetBetweenAsync(request.UserId, request.FriendUserId, cancellationToken)
                         ?? throw NotFoundException.For("Friendship", request.FriendUserId);

        if (!friendship.Involves(request.UserId))
        {
            throw new ForbiddenException("You cannot remove this friendship.");
        }

        _friendships.Remove(friendship);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
