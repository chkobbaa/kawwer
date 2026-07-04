using FluentValidation;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Entities;

namespace Kawwer.Application.Features.Push;

/// <summary>Registers (or refreshes) the caller's Web Push subscription for the PWA.</summary>
public sealed record SubscribeWebPushCommand(Guid UserId, string Endpoint, string P256dh, string Auth)
    : IRequest<Unit>;

public sealed class SubscribeWebPushCommandValidator : AbstractValidator<SubscribeWebPushCommand>
{
    public SubscribeWebPushCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Endpoint).NotEmpty().MaximumLength(1000)
            .Must(e => Uri.TryCreate(e, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps)
            .WithMessage("A valid HTTPS push endpoint is required.");
        RuleFor(x => x.P256dh).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Auth).NotEmpty().MaximumLength(255);
    }
}

public sealed class SubscribeWebPushCommandHandler : IRequestHandler<SubscribeWebPushCommand, Unit>
{
    private readonly IPushSubscriptionRepository _subscriptions;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public SubscribeWebPushCommandHandler(
        IPushSubscriptionRepository subscriptions,
        IUserRepository users,
        IUnitOfWork unitOfWork)
    {
        _subscriptions = subscriptions;
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(SubscribeWebPushCommand request, CancellationToken cancellationToken)
    {
        _ = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw NotFoundException.For("User", request.UserId);

        // A browser reuses one endpoint per push service, so an existing row means the same device
        // is re-subscribing: re-point it at the caller and refresh its keys instead of duplicating.
        var existing = await _subscriptions.GetByEndpointAsync(request.Endpoint, cancellationToken);
        if (existing is not null)
        {
            existing.AssignTo(request.UserId, request.P256dh, request.Auth);
        }
        else
        {
            _subscriptions.Add(new PushSubscription(request.UserId, request.Endpoint, request.P256dh, request.Auth));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
