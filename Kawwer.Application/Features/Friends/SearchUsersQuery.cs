using FluentValidation;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Users;

namespace Kawwer.Application.Features.Friends;

public sealed record SearchUsersQuery(Guid UserId, string Term) : IRequest<IReadOnlyList<UserSummaryDto>>;

public sealed class SearchUsersQueryValidator : AbstractValidator<SearchUsersQuery>
{
    public SearchUsersQueryValidator()
    {
        RuleFor(x => x.Term).NotEmpty().MinimumLength(2).MaximumLength(50);
    }
}

public sealed class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, IReadOnlyList<UserSummaryDto>>
{
    private const int MaxResults = 25;
    private readonly IUserRepository _users;

    public SearchUsersQueryHandler(IUserRepository users) => _users = users;

    public async Task<IReadOnlyList<UserSummaryDto>> HandleAsync(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _users.SearchAsync(request.Term, MaxResults, cancellationToken);
        return users
            .Where(u => u.Id != request.UserId)
            .Select(u => u.ToSummaryDto())
            .ToList();
    }
}
