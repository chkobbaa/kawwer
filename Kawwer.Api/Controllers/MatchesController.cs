using Kawwer.Application.Features.LiveMatch;
using Kawwer.Application.Features.Matches;
using Kawwer.Contracts.Matches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kawwer.Api.Controllers;

[Authorize]
[Route("api/v1/matches")]
public sealed class MatchesController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateMatchRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateMatchCommand(
            CurrentUserId,
            request.FootballFieldId,
            request.Title,
            request.Description,
            request.MatchDate,
            request.StartTime,
            request.MaxPlayers,
            request.TotalFieldPrice,
            request.Visibility,
            request.AutoAcceptPublic,
            request.InvitedUserIds,
            request.InvitedTeamIds,
            request.Format,
            request.OpponentName,
            request.OpponentTeamId);

        var id = await Dispatcher.SendAsync(command, cancellationToken);
        return CreatedResponse(id, "Match published.");
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetOrganizerDashboardQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> Upcoming(CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetUpcomingMatchesQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetMatchQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/participants")]
    public async Task<IActionResult> GetParticipants(Guid id, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetMatchParticipantsQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMatchRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateMatchCommand(
            CurrentUserId, id, request.MatchDate, request.StartTime, request.DurationMinutes, request.Description, request.Visibility);
        await Dispatcher.SendAsync(command, cancellationToken);
        return OkMessage("Match updated.");
    }

    [HttpPut("{id:guid}/capacity")]
    public async Task<IActionResult> ChangeCapacity(Guid id, ChangeCapacityRequest request, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new ChangeCapacityCommand(CurrentUserId, id, request.MaxPlayers), cancellationToken);
        return OkMessage("Capacity updated.");
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new CancelMatchCommand(CurrentUserId, id), cancellationToken);
        return OkMessage("Match cancelled.");
    }

    [HttpPost("{id:guid}/finish")]
    public async Task<IActionResult> Finish(Guid id, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new FinishMatchCommand(CurrentUserId, id), cancellationToken);
        return OkMessage("Match finished.");
    }

    [HttpPost("{id:guid}/invitations")]
    public async Task<IActionResult> Invite(Guid id, InvitePlayersRequest request, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new InvitePlayersCommand(CurrentUserId, id, request.UserIds, request.TeamIds), cancellationToken);
        return OkMessage("Invitations sent.");
    }

    [HttpPost("{id:guid}/respond")]
    public async Task<IActionResult> Respond(Guid id, RespondToInvitationRequest request, CancellationToken cancellationToken)
    {
        var joined = await Dispatcher.SendAsync(new RespondToInvitationCommand(CurrentUserId, id, request.Accept), cancellationToken);
        return Ok(new { joined }, request.Accept
            ? joined ? "You joined the match." : "You were added to the waiting list."
            : "You declined the invitation.");
    }

    [HttpPost("{id:guid}/seen")]
    public async Task<IActionResult> MarkSeen(Guid id, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new MarkInvitationStatusCommand(CurrentUserId, id, Thinking: false), cancellationToken);
        return OkMessage();
    }

    [HttpPost("{id:guid}/thinking")]
    public async Task<IActionResult> MarkThinking(Guid id, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new MarkInvitationStatusCommand(CurrentUserId, id, Thinking: true), cancellationToken);
        return OkMessage();
    }

    [HttpPost("{id:guid}/leave")]
    public async Task<IActionResult> Leave(Guid id, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new LeaveMatchCommand(CurrentUserId, id), cancellationToken);
        return OkMessage("You left the match.");
    }

    [HttpGet("{id:guid}/waiting-list")]
    public async Task<IActionResult> WaitingListPosition(Guid id, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetWaitingListPositionQuery(CurrentUserId, id), cancellationToken);
        return Ok(result);
    }

    // ----- Live Match -----

    [HttpPost("{id:guid}/live/start")]
    public async Task<IActionResult> StartLive(Guid id, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new StartLiveMatchCommand(CurrentUserId, id), cancellationToken);
        return OkMessage("Live Match started.");
    }

    [HttpPost("{id:guid}/live/attendance")]
    public async Task<IActionResult> UpdateAttendance(Guid id, UpdateAttendanceRequest request, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new UpdateAttendanceCommand(CurrentUserId, id, request.UserId, request.Attendance), cancellationToken);
        return OkMessage("Attendance updated.");
    }

    [HttpPost("{id:guid}/live/location")]
    public async Task<IActionResult> ShareLocation(Guid id, ShareLocationRequest request, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new ShareLocationCommand(CurrentUserId, id, request.Latitude, request.Longitude), cancellationToken);
        return OkMessage("Location shared.");
    }

    [HttpDelete("{id:guid}/live/location")]
    public async Task<IActionResult> StopSharingLocation(Guid id, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new StopSharingLocationCommand(CurrentUserId, id), cancellationToken);
        return OkMessage("Location sharing stopped.");
    }

    [HttpPost("{id:guid}/live/request-locations")]
    public async Task<IActionResult> RequestLocations(Guid id, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new RequestLocationsCommand(CurrentUserId, id), cancellationToken);
        return OkMessage("Location requests sent.");
    }
}
