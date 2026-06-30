using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Friends;

public sealed record AcceptFriendRequestCommand(Guid UserId, Guid FriendshipId) : IRequest<Unit>;

public sealed class AcceptFriendRequestCommandHandler : IRequestHandler<AcceptFriendRequestCommand, Unit>
{
    private readonly IFriendshipRepository _friendships;
    private readonly IUserRepository _users;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptFriendRequestCommandHandler(
        IFriendshipRepository friendships,
        IUserRepository users,
        INotificationService notifications,
        IUnitOfWork unitOfWork)
    {
        _friendships = friendships;
        _users = users;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(AcceptFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendship = await _friendships.GetByIdAsync(request.FriendshipId, cancellationToken)
                         ?? throw NotFoundException.For("Friend request", request.FriendshipId);

        // Only the recipient of the request may accept it.
        if (friendship.FriendId != request.UserId)
        {
            throw new ForbiddenException("You cannot accept this friend request.");
        }

        friendship.Accept();

        var accepter = await _users.GetByIdAsync(request.UserId, cancellationToken);
        await _notifications.NotifyAsync(
            friendship.UserId,
            NotificationCategory.Friend,
            "Friend request accepted",
            $"{accepter?.FullName ?? "Someone"} accepted your friend request.",
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
