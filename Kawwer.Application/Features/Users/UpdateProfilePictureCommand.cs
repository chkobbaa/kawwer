using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Realtime;
using Kawwer.Contracts.Users;

namespace Kawwer.Application.Features.Users;

/// <summary>Persists the URL of an already-stored profile picture for the given user.</summary>
public sealed record UpdateProfilePictureCommand(Guid UserId, string Url) : IRequest<UserDto>;

public sealed class UpdateProfilePictureCommandHandler : IRequestHandler<UpdateProfilePictureCommand, UserDto>
{
    private readonly IUserRepository _users;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfilePictureCommandHandler(IUserRepository users, IRealtimeNotifier realtime, IUnitOfWork unitOfWork)
    {
        _users = users;
        _realtime = realtime;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto> HandleAsync(UpdateProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
                   ?? throw NotFoundException.For("User", request.UserId);

        user.SetProfilePicture(request.Url);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Keep the user's other open sessions in sync so the new photo appears everywhere.
        await _realtime.NotifyUserAsync(request.UserId, new RealtimeUserEvent("Profile"), cancellationToken);

        return user.ToDto();
    }
}
