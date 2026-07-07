using Kawwer.Application.Features.Teams;
using Kawwer.Contracts.Teams;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kawwer.Api.Controllers;

[Authorize]
[Route("api/v1/teams")]
public sealed class TeamsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetTeamsQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetTeamQuery(CurrentUserId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTeamRequest request, CancellationToken cancellationToken)
    {
        var id = await Dispatcher.SendAsync(new CreateTeamCommand(CurrentUserId, request.Name, request.Description), cancellationToken);
        return CreatedResponse(id, "Team created.");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTeamRequest request, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new UpdateTeamCommand(CurrentUserId, id, request.Name, request.Description), cancellationToken);
        return OkMessage("Team updated.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new DeleteTeamCommand(CurrentUserId, id), cancellationToken);
        return OkMessage("Team deleted.");
    }

    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMember(Guid id, AddTeamMemberRequest request, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new AddTeamMemberCommand(CurrentUserId, id, request.UserId), cancellationToken);
        return OkMessage("Member added.");
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new RemoveTeamMemberCommand(CurrentUserId, id, userId), cancellationToken);
        return OkMessage("Member removed.");
    }
}
