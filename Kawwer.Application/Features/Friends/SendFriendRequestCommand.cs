using FluentValidation;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Friends;

public sealed record SendFriendRequestCommand(Guid RequesterId, Guid TargetUserId) : IRequest<Guid>;

public sealed class SendFriendRequestCommandValidator : AbstractValidator<SendFriendRequestCommand>
{
    public SendFriendRequestCommandValidator()
    {
        RuleFor(x => x.TargetUserId).NotEmpty();
        RuleFor(x => x).Must(x => x.RequesterId != x.TargetUserId)
            .WithMessage("You cannot send a friend request to yourself.");
    }
}

public sealed class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommand, Guid>
{
    private readonly IFriendshipRepository _friendships;
    private readonly IUserRepository _users;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public SendFriendRequestCommandHandler(
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

    public async Task<Guid> HandleAsync(SendFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var requester = await _users.GetByIdAsync(request.RequesterId, cancellationToken)
                        ?? throw NotFoundException.For("User", request.RequesterId);
        var target = await _users.GetByIdAsync(request.TargetUserId, cancellationToken)
                     ?? throw NotFoundException.For("User", request.TargetUserId);

        var existing = await _friendships.GetBetweenAsync(requester.Id, target.Id, cancellationToken);
        if (existing is not null)
        {
            if (existing.Status == FriendshipStatus.Blocked)
            {
                throw new ForbiddenException("You cannot send a friend request to this user.");
            }

            throw new ConflictException("A friend request or friendship already exists.");
        }

        var friendship = new Friendship(requester.Id, target.Id);
        _friendships.Add(friendship);

        await _notifications.NotifyAsync(
            target.Id,
            NotificationCategory.Friend,
            "New friend request",
            $"{requester.FullName} sent you a friend request.",
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return friendship.Id;
    }
}
