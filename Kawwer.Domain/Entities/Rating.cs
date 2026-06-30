using Kawwer.Domain.Common;
using Kawwer.Domain.Enums;
using Kawwer.Domain.Exceptions;

namespace Kawwer.Domain.Entities;

/// <summary>
/// An anonymous five-star rating submitted after a finished match. One per rater/ratee/type/match.
/// </summary>
public class Rating : Entity
{
    private Rating()
    {
    }

    public Rating(Guid matchId, Guid raterId, Guid rateeId, RatingType type, int stars, string? comment = null)
    {
        if (stars is < 1 or > 5)
        {
            throw new DomainException("A rating must be between one and five stars.");
        }

        if (raterId == rateeId)
        {
            throw new DomainException("A user cannot rate themselves.");
        }

        Id = Guid.NewGuid();
        MatchId = matchId;
        RaterId = raterId;
        RateeId = rateeId;
        Type = type;
        Stars = stars;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid MatchId { get; private set; }
    public Guid RaterId { get; private set; }
    public Guid RateeId { get; private set; }
    public RatingType Type { get; private set; }
    public int Stars { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
