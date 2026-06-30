using Kawwer.Contracts.Users;
using Kawwer.Domain.Enums;

namespace Kawwer.Contracts.Payments;

public sealed record RecordPaymentRequest(Guid UserId, decimal Amount);

public sealed record MarkPaidRequest(Guid UserId);

public sealed record UndoPaymentRequest(Guid UserId);

public sealed record AssignBalanceRequest(IReadOnlyList<Guid> UserIds);

public sealed record PaymentPlayerDto(
    Guid UserId,
    UserSummaryDto User,
    decimal PaidAmount,
    decimal Share,
    PaymentStatus Status);

public sealed record PaymentSummaryDto(
    Guid MatchId,
    decimal TotalFieldPrice,
    decimal ReservationPaid,
    decimal RemainingAmount,
    decimal CollectedAmount,
    decimal MissingAmount,
    decimal SharePerPlayer,
    int PaidPlayers,
    int PartialPlayers,
    int UnpaidPlayers,
    bool CollectionStarted,
    bool CollectionCompleted,
    IReadOnlyList<PaymentPlayerDto> Players);
