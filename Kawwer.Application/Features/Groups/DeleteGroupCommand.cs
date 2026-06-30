using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Groups;

public sealed record DeleteGroupCommand(Guid OwnerId, Guid GroupId) : IRequest<Unit>;

public sealed class DeleteGroupCommandHandler : IRequestHandler<DeleteGroupCommand, Unit>
{
    private readonly IGroupRepository _groups;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteGroupCommandHandler(IGroupRepository groups, IUnitOfWork unitOfWork)
    {
        _groups = groups;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(DeleteGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _groups.GetByIdAsync(request.GroupId, cancellationToken)
                    ?? throw NotFoundException.For("Group", request.GroupId);

        if (group.OwnerId != request.OwnerId)
        {
            throw new ForbiddenException("Only the group owner can delete this group.");
        }

        _groups.Remove(group);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
