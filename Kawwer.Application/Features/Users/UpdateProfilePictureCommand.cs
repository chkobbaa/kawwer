using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Users;

namespace Kawwer.Application.Features.Users;

/// <summary>Persists the URL of an already-stored profile picture for the given user.</summary>
public sealed record UpdateProfilePictureCommand(Guid UserId, string Url) : IRequest<UserDto>;

public sealed class UpdateProfilePictureCommandHandler : IRequestHandler<UpdateProfilePictureCommand, UserDto>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfilePictureCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto> HandleAsync(UpdateProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
                   ?? throw NotFoundException.For("User", request.UserId);

        user.SetProfilePicture(request.Url);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return user.ToDto();
    }
}
