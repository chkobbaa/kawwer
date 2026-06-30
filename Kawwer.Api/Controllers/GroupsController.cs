using Kawwer.Application.Features.Groups;
using Kawwer.Contracts.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kawwer.Api.Controllers;

[Authorize]
[Route("api/v1/groups")]
public sealed class GroupsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetGroupsQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetGroupQuery(CurrentUserId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateGroupRequest request, CancellationToken cancellationToken)
    {
        var id = await Dispatcher.SendAsync(new CreateGroupCommand(CurrentUserId, request.Name, request.Description), cancellationToken);
        return CreatedResponse(id, "Group created.");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateGroupRequest request, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new UpdateGroupCommand(CurrentUserId, id, request.Name, request.Description), cancellationToken);
        return OkMessage("Group updated.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new DeleteGroupCommand(CurrentUserId, id), cancellationToken);
        return OkMessage("Group deleted.");
    }

    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMember(Guid id, AddGroupMemberRequest request, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new AddGroupMemberCommand(CurrentUserId, id, request.UserId), cancellationToken);
        return OkMessage("Member added.");
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new RemoveGroupMemberCommand(CurrentUserId, id, userId), cancellationToken);
        return OkMessage("Member removed.");
    }
}
