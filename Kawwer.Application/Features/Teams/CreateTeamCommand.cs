using FluentValidation;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Entities;

namespace Kawwer.Application.Features.Teams;

public sealed record CreateTeamCommand(Guid OwnerId, string Name, string? Description) : IRequest<Guid>;

public sealed class CreateTeamCommandValidator : AbstractValidator<CreateTeamCommand>
{
    public CreateTeamCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}

public sealed class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, Guid>
{
    private readonly ITeamRepository _teams;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTeamCommandHandler(ITeamRepository teams, IUnitOfWork unitOfWork)
    {
        _teams = teams;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        var team = new Team(request.OwnerId, request.Name, request.Description);
        _teams.Add(team);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return team.Id;
    }
}
