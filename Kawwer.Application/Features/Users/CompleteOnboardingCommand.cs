using FluentValidation;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Users;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Users;

/// <summary>
/// Saves the answers gathered during the first-run onboarding flow (date of birth, preferred
/// playing position and preferred foot) for the current user and marks onboarding as completed.
/// </summary>
public sealed record CompleteOnboardingCommand(
    Guid UserId,
    DateOnly? BirthDate,
    PreferredPosition? PreferredPosition,
    PreferredFoot? PreferredFoot) : IRequest<UserDto>;

public sealed class CompleteOnboardingCommandValidator : AbstractValidator<CompleteOnboardingCommand>
{
    // Sanity bounds for a footballer's date of birth: no future dates and a plausible age range.
    private const int MinAgeYears = 10;
    private const int MaxAgeYears = 100;

    public CompleteOnboardingCommandValidator()
    {
        RuleFor(x => x.BirthDate)
            .NotNull().WithMessage("Your date of birth is required.")
            .Must(BeAPlausibleBirthDate).WithMessage("Enter a valid date of birth.")
            .When(x => x.BirthDate.HasValue);

        RuleFor(x => x.PreferredPosition)
            .NotNull().WithMessage("Choose your preferred position.")
            .IsInEnum();

        RuleFor(x => x.PreferredFoot)
            .NotNull().WithMessage("Choose your preferred foot.")
            .IsInEnum();
    }

    private static bool BeAPlausibleBirthDate(DateOnly? birthDate)
    {
        if (birthDate is not { } value)
        {
            return false;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return value <= today.AddYears(-MinAgeYears)
               && value >= today.AddYears(-MaxAgeYears);
    }
}

public sealed class CompleteOnboardingCommandHandler : IRequestHandler<CompleteOnboardingCommand, UserDto>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteOnboardingCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto> HandleAsync(CompleteOnboardingCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
                   ?? throw NotFoundException.For("User", request.UserId);

        user.CompleteOnboarding(request.BirthDate, request.PreferredPosition, request.PreferredFoot);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return user.ToDto();
    }
}
