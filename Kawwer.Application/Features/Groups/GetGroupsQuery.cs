using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Groups;

namespace Kawwer.Application.Features.Groups;

public sealed record GetGroupsQuery(Guid OwnerId) : IRequest<IReadOnlyList<GroupDto>>;

public sealed class GetGroupsQueryHandler : IRequestHandler<GetGroupsQuery, IReadOnlyList<GroupDto>>
{
    private readonly IGroupRepository _groups;
    private readonly IUserRepository _users;

    public GetGroupsQueryHandler(IGroupRepository groups, IUserRepository users)
    {
        _groups = groups;
        _users = users;
    }

    public async Task<IReadOnlyList<GroupDto>> HandleAsync(GetGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = await _groups.GetForOwnerAsync(request.OwnerId, cancellationToken);

        var memberIds = groups.SelectMany(g => g.Members.Select(m => m.UserId)).Distinct().ToList();
        var users = (await _users.GetByIdsAsync(memberIds, cancellationToken)).ToDictionary(u => u.Id);

        return groups.Select(g => g.ToDto(users)).ToList();
    }
}
