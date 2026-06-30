using Kawwer.Application.Features.FootballFields;
using Kawwer.Contracts.Common;
using Kawwer.Contracts.FootballFields;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kawwer.Api.Controllers;

[Authorize]
[Route("api/v1/football-fields")]
public sealed class FootballFieldsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? search, [FromQuery] PaginationQuery pagination, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(
            new SearchFootballFieldsQuery(search, pagination.Page, pagination.PageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetFootballFieldQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateFootballFieldRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateFootballFieldCommand(
            CurrentUserId,
            request.Name,
            request.Address,
            request.Latitude,
            request.Longitude,
            request.Capacity,
            request.MatchDurationMinutes,
            request.Price,
            request.ReservationFee,
            request.Surface,
            request.Indoor,
            request.Parking,
            request.Shower,
            request.Lights,
            request.PhoneNumber,
            request.GoogleMapsUrl,
            request.Notes);

        var id = await Dispatcher.SendAsync(command, cancellationToken);
        return CreatedResponse(id, "Football field created.");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateFootballFieldRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateFootballFieldCommand(
            CurrentUserId,
            id,
            request.Name,
            request.Address,
            request.Latitude,
            request.Longitude,
            request.Capacity,
            request.MatchDurationMinutes,
            request.Price,
            request.ReservationFee,
            request.Surface,
            request.Indoor,
            request.Parking,
            request.Shower,
            request.Lights,
            request.PhoneNumber,
            request.GoogleMapsUrl,
            request.Notes);

        await Dispatcher.SendAsync(command, cancellationToken);
        return OkMessage("Football field updated.");
    }
}
