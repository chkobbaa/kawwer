using FluentValidation;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Entities;

namespace Kawwer.Application.Features.Groups;

public sealed record CreateGroupCommand(Guid OwnerId, string Name, string? Description) : IRequest<Guid>;

public sealed class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}

public sealed class CreateGroupCommandHandler : IRequestHandler<CreateGroupCommand, Guid>
{
    private readonly IGroupRepository _groups;
    private readonly IUnitOfWork _unitOfWork;

    public CreateGroupCommandHandler(IGroupRepository groups, IUnitOfWork unitOfWork)
    {
        _groups = groups;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var group = new Group(request.OwnerId, request.Name, request.Description);
        _groups.Add(group);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return group.Id;
    }
}
