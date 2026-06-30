using FluentValidation;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Ratings;

public sealed record RatingInput(Guid RateeId, RatingType Type, int Stars, string? Comment);

public sealed record SubmitMatchRatingsCommand(Guid RaterId, Guid MatchId, IReadOnlyList<RatingInput> Ratings)
    : IRequest<Unit>;

public sealed class SubmitMatchRatingsCommandValidator : AbstractValidator<SubmitMatchRatingsCommand>
{
    public SubmitMatchRatingsCommandValidator()
    {
        RuleFor(x => x.Ratings).NotEmpty();
        RuleForEach(x => x.Ratings).ChildRules(r =>
        {
            r.RuleFor(x => x.Stars).InclusiveBetween(1, 5);
            r.RuleFor(x => x.Comment).MaximumLength(500);
        });
    }
}

public sealed class SubmitMatchRatingsCommandHandler : IRequestHandler<SubmitMatchRatingsCommand, Unit>
{
    // A five-star scale centred on three: 5 stars => +4 reputation, 1 star => -4.
    private const decimal ReputationPerStar = 2m;

    private readonly IMatchRepository _matches;
    private readonly IRatingRepository _ratings;
    private readonly IUserRepository _users;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitMatchRatingsCommandHandler(
        IMatchRepository matches,
        IRatingRepository ratings,
        IUserRepository users,
        IDateTimeProvider clock,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _ratings = ratings;
        _users = users;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(SubmitMatchRatingsCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.Status != MatchStatus.Finished)
        {
            throw new ConflictException("Ratings open only after a match is finished.");
        }

        if (_clock.UtcNow - match.UpdatedAt > TimeSpan.FromDays(7))
        {
            throw new ConflictException("The seven-day rating window for this match has closed.");
        }

        var isParticipant = match.OrganizerId == request.RaterId
                            || match.Participants.Any(p => p.UserId == request.RaterId && p.Status == ParticipantStatus.Accepted);
        if (!isParticipant)
        {
            throw new ForbiddenException("Only the organizer or accepted players can rate this match.");
        }

        foreach (var input in request.Ratings)
        {
            if (input.RateeId == request.RaterId)
            {
                continue;
            }

            if (await _ratings.ExistsAsync(match.Id, request.RaterId, input.RateeId, input.Type, cancellationToken))
            {
                continue;
            }

            _ratings.Add(new Rating(match.Id, request.RaterId, input.RateeId, input.Type, input.Stars, input.Comment));

            var ratee = await _users.GetByIdAsync(input.RateeId, cancellationToken);
            ratee?.AdjustReputation((input.Stars - 3) * ReputationPerStar);
        }

        // Record that this rater has submitted, so the UI can hide the rating prompt.
        var raterParticipant = match.Participants.FirstOrDefault(p => p.UserId == request.RaterId);
        if (raterParticipant is not null)
        {
            if (request.Ratings.Any(r => r.Type == RatingType.Organizer))
            {
                raterParticipant.MarkRatedOrganizer();
            }

            if (request.Ratings.Any(r => r.Type == RatingType.Player))
            {
                raterParticipant.MarkRatedPlayers();
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
