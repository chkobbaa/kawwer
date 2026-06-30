using FluentValidation;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Groups;

public sealed record UpdateGroupCommand(Guid OwnerId, Guid GroupId, string Name, string? Description) : IRequest<Unit>;

public sealed class UpdateGroupCommandValidator : AbstractValidator<UpdateGroupCommand>
{
    public UpdateGroupCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}

public sealed class UpdateGroupCommandHandler : IRequestHandler<UpdateGroupCommand, Unit>
{
    private readonly IGroupRepository _groups;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateGroupCommandHandler(IGroupRepository groups, IUnitOfWork unitOfWork)
    {
        _groups = groups;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _groups.GetByIdAsync(request.GroupId, cancellationToken)
                    ?? throw NotFoundException.For("Group", request.GroupId);

        if (group.OwnerId != request.OwnerId)
        {
            throw new ForbiddenException("Only the group owner can modify this group.");
        }

        group.Rename(request.Name, request.Description);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
