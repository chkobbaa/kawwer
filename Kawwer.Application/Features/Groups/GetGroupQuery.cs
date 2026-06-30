using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Groups;

namespace Kawwer.Application.Features.Groups;

public sealed record GetGroupQuery(Guid OwnerId, Guid GroupId) : IRequest<GroupDto>;

public sealed class GetGroupQueryHandler : IRequestHandler<GetGroupQuery, GroupDto>
{
    private readonly IGroupRepository _groups;
    private readonly IUserRepository _users;

    public GetGroupQueryHandler(IGroupRepository groups, IUserRepository users)
    {
        _groups = groups;
        _users = users;
    }

    public async Task<GroupDto> HandleAsync(GetGroupQuery request, CancellationToken cancellationToken)
    {
        var group = await _groups.GetByIdAsync(request.GroupId, cancellationToken)
                    ?? throw NotFoundException.For("Group", request.GroupId);

        if (group.OwnerId != request.OwnerId)
        {
            throw new ForbiddenException("Only the group owner can view this group.");
        }

        var memberIds = group.Members.Select(m => m.UserId).ToList();
        var users = (await _users.GetByIdsAsync(memberIds, cancellationToken)).ToDictionary(u => u.Id);
        return group.ToDto(users);
    }
}
