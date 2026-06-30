using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Entities;

namespace Kawwer.Application.Features.Friends;

/// <summary>Blocks a user. Any existing friendship is replaced by a block record (blocker -&gt; blocked).</summary>
public sealed record BlockUserCommand(Guid UserId, Guid TargetUserId) : IRequest<Unit>;

public sealed class BlockUserCommandHandler : IRequestHandler<BlockUserCommand, Unit>
{
    private readonly IFriendshipRepository _friendships;
    private readonly IUnitOfWork _unitOfWork;

    public BlockUserCommandHandler(IFriendshipRepository friendships, IUnitOfWork unitOfWork)
    {
        _friendships = friendships;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(BlockUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == request.TargetUserId)
        {
            throw new ForbiddenException("You cannot block yourself.");
        }

        var existing = await _friendships.GetBetweenAsync(request.UserId, request.TargetUserId, cancellationToken);
        if (existing is not null)
        {
            _friendships.Remove(existing);
        }

        var block = new Friendship(request.UserId, request.TargetUserId);
        block.Block();
        _friendships.Add(block);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
