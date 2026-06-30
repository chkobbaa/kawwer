using Kawwer.Application.Features.Ratings;
using Kawwer.Contracts.Ratings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kawwer.Api.Controllers;

[Authorize]
[Route("api/v1/matches/{matchId:guid}/ratings")]
public sealed class RatingsController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit(Guid matchId, SubmitMatchRatingsRequest request, CancellationToken cancellationToken)
    {
        var ratings = request.Ratings
            .Select(r => new RatingInput(r.RateeId, r.Type, r.Stars, r.Comment))
            .ToList();

        await Dispatcher.SendAsync(new SubmitMatchRatingsCommand(CurrentUserId, matchId, ratings), cancellationToken);
        return OkMessage("Ratings submitted.");
    }
}
