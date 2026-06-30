using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Users;

/// <summary>Registers (or clears) the caller's FCM device token for push notifications.</summary>
public sealed record UpdateDeviceTokenCommand(Guid UserId, string? DeviceToken) : IRequest<Unit>;

public sealed class UpdateDeviceTokenCommandHandler : IRequestHandler<UpdateDeviceTokenCommand, Unit>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDeviceTokenCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(UpdateDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
                   ?? throw NotFoundException.For("User", request.UserId);

        user.SetDeviceToken(request.DeviceToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
