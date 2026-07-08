using Kawwer.Application.Common.Interfaces;
using Kawwer.Domain.Enums;
using Kawwer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kawwer.Infrastructure.BackgroundJobs;

/// <summary>
/// Periodically closes matches whose scheduled end has passed, moving still-open matches (draft,
/// published, full, or in-play) to <see cref="MatchStatus.Expired"/>. Expiring a match also purges
/// its now-meaningless invitation notifications, so players never see stale Accept/Decline actions
/// for a game that already came and went — and the organizer is never pinged about a response to
/// a match that no longer exists.
///
/// Kickoff/end are interpreted in the app time zone (via <see cref="IDateTimeProvider"/>), so a
/// match scheduled for "20:00" expires at 20:00 local — not at 20:00 UTC.
/// </summary>
public sealed class MatchLifecycleService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MatchLifecycleService> _logger;

    public MatchLifecycleService(IServiceScopeFactory scopeFactory, ILogger<MatchLifecycleService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Match expiry sweep failed.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<KawwerDbContext>();
        var notifications = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var clock = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var now = clock.UtcNow;
        var zone = clock.AppTimeZone;
        // Only non-terminal matches whose wall-clock date is today or already in the past can be due
        // to expire. (A one-day back window covers the app time-zone offset.)
        var cutoffDate = DateOnly.FromDateTime(now.AddDays(1));

        var candidates = await context.Matches
            .Where(m => (m.Status == MatchStatus.Draft
                         || m.Status == MatchStatus.Published
                         || m.Status == MatchStatus.Full
                         || m.Status == MatchStatus.Playing)
                        && m.MatchDate <= cutoffDate)
            .ToListAsync(cancellationToken);

        var expiredIds = new List<Guid>();
        foreach (var match in candidates)
        {
            if (match.TryExpire(now, zone))
            {
                expiredIds.Add(match.Id);
            }
        }

        if (expiredIds.Count == 0)
        {
            return;
        }

        foreach (var matchId in expiredIds)
        {
            // Drop lingering invitations so no one can act on (or be notified about) a dead match.
            await notifications.RemoveForMatchAsync(matchId, NotificationCategory.Invitation, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Expired {Count} match(es) past their scheduled end.", expiredIds.Count);
    }
}
