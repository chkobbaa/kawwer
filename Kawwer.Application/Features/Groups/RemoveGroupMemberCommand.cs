using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Groups;

public sealed record RemoveGroupMemberCommand(Guid OwnerId, Guid GroupId, Guid MemberUserId) : IRequest<Unit>;

public sealed class RemoveGroupMemberCommandHandler : IRequestHandler<RemoveGroupMemberCommand, Unit>
{
    private readonly IGroupRepository _groups;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveGroupMemberCommandHandler(IGroupRepository groups, IUnitOfWork unitOfWork)
    {
        _groups = groups;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(RemoveGroupMemberCommand request, CancellationToken cancellationToken)
    {
        var group = await _groups.GetByIdAsync(request.GroupId, cancellationToken)
                    ?? throw NotFoundException.For("Group", request.GroupId);

        if (group.OwnerId != request.OwnerId)
        {
            throw new ForbiddenException("Only the group owner can remove members.");
        }

        group.RemoveMember(request.MemberUserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
