using FluentValidation;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Push;

/// <summary>Removes the caller's Web Push subscription (e.g. they turned notifications off or logged out).</summary>
public sealed record UnsubscribeWebPushCommand(Guid UserId, string Endpoint) : IRequest<Unit>;

public sealed class UnsubscribeWebPushCommandValidator : AbstractValidator<UnsubscribeWebPushCommand>
{
    public UnsubscribeWebPushCommandValidator()
    {
        RuleFor(x => x.Endpoint).NotEmpty();
    }
}

public sealed class UnsubscribeWebPushCommandHandler : IRequestHandler<UnsubscribeWebPushCommand, Unit>
{
    private readonly IPushSubscriptionRepository _subscriptions;
    private readonly IUnitOfWork _unitOfWork;

    public UnsubscribeWebPushCommandHandler(IPushSubscriptionRepository subscriptions, IUnitOfWork unitOfWork)
    {
        _subscriptions = subscriptions;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(UnsubscribeWebPushCommand request, CancellationToken cancellationToken)
    {
        var existing = await _subscriptions.GetByEndpointAsync(request.Endpoint, cancellationToken);

        // Only the owner may remove their subscription; silently ignore anything else so the
        // client's "disable notifications" call is always idempotent.
        if (existing is not null && existing.UserId == request.UserId)
        {
            _subscriptions.Remove(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
