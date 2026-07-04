using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Users;

/// <summary>
/// Deletes (deactivates) the caller's own account. This is a soft delete: the account is marked
/// <c>Deleted</c> and every active refresh token is revoked, which immediately ends the session and
/// blocks future logins/refreshes (both check <see cref="Domain.Entities.User.IsActive"/>). A soft
/// delete keeps the user's history (matches, ratings, payments) referentially intact.
/// </summary>
public sealed record DeleteAccountCommand(Guid UserId) : IRequest<Unit>;

public sealed class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, Unit>
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAccountCommandHandler(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
                   ?? throw NotFoundException.For("User", request.UserId);

        foreach (var token in await _refreshTokens.GetActiveForUserAsync(user.Id, cancellationToken))
        {
            token.Revoke();
        }

        user.SoftDelete();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
