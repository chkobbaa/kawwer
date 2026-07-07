using FluentValidation;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Teams;

public sealed record UpdateTeamCommand(Guid OwnerId, Guid TeamId, string Name, string? Description) : IRequest<Unit>;

public sealed class UpdateTeamCommandValidator : AbstractValidator<UpdateTeamCommand>
{
    public UpdateTeamCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}

public sealed class UpdateTeamCommandHandler : IRequestHandler<UpdateTeamCommand, Unit>
{
    private readonly ITeamRepository _teams;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTeamCommandHandler(ITeamRepository teams, IUnitOfWork unitOfWork)
    {
        _teams = teams;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(UpdateTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await _teams.GetByIdAsync(request.TeamId, cancellationToken)
                   ?? throw NotFoundException.For("Team", request.TeamId);

        if (team.OwnerId != request.OwnerId)
        {
            throw new ForbiddenException("Only the team owner can modify this team.");
        }

        team.Rename(request.Name, request.Description);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
