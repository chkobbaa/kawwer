using Kawwer.Domain.Enums;

namespace Kawwer.Contracts.Ratings;

public sealed record SubmitRatingRequest(
    Guid RateeId,
    RatingType Type,
    int Stars,
    string? Comment);

public sealed record SubmitMatchRatingsRequest(IReadOnlyList<SubmitRatingRequest> Ratings);
