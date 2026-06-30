using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Users;

namespace Kawwer.Application.Features.Users;

public sealed record GetProfileQuery(Guid UserId) : IRequest<UserDto>;

public sealed class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserDto>
{
    private readonly IUserRepository _users;

    public GetProfileQueryHandler(IUserRepository users) => _users = users;

    public async Task<UserDto> HandleAsync(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
                   ?? throw NotFoundException.For("User", request.UserId);
        return user.ToDto();
    }
}
