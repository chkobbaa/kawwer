using FluentValidation;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Users;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Users;

public sealed record UpdateProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    DateOnly? BirthDate,
    PreferredPosition? PreferredPosition,
    PreferredFoot? PreferredFoot,
    int? SkillLevel,
    ProfileVisibility Visibility) : IRequest<UserDto>;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SkillLevel).InclusiveBetween(1, 10).When(x => x.SkillLevel.HasValue);
    }
}

public sealed class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserDto>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto> HandleAsync(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
                   ?? throw NotFoundException.For("User", request.UserId);

        user.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.BirthDate,
            request.PreferredPosition,
            request.PreferredFoot,
            request.SkillLevel,
            request.Visibility);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return user.ToDto();
    }
}
