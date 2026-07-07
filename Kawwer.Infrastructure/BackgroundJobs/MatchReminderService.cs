using Kawwer.Application.Common.Interfaces;
using Kawwer.Domain.Enums;
using Kawwer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kawwer.Infrastructure.BackgroundJobs;

/// <summary>
/// Periodically sends automatic match reminders (24h, 3h and 30m before kickoff) to players who
/// have not declined. Best-effort de-duplication keeps a single reminder per window per process run.
/// </summary>
public sealed class MatchReminderService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(5);

    private static readonly (string Label, TimeSpan Before)[] Windows =
    {
        ("24h", TimeSpan.FromHours(24)),
        ("3h", TimeSpan.FromHours(3)),
        ("30m", TimeSpan.FromMinutes(30))
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MatchReminderService> _logger;
    private readonly HashSet<string> _sent = new();

    public MatchReminderService(IServiceScopeFactory scopeFactory, ILogger<MatchReminderService> logger)
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
                _logger.LogError(ex, "Match reminder sweep failed.");
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
        var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var clock = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var now = clock.UtcNow;
        var zone = clock.AppTimeZone;
        // Look one day back too: a match's wall-clock date can still be "today" in UTC terms while
        // its real kickoff (offset by the app time zone) is imminent.
        var earliest = DateOnly.FromDateTime(now.AddDays(-1));

        var matches = await context.Matches
            .Include(m => m.Participants)
            .Where(m => m.MatchDate >= earliest
                        && (m.Status == MatchStatus.Published || m.Status == MatchStatus.Full))
            .ToListAsync(cancellationToken);

        var anySent = false;

        foreach (var match in matches)
        {
            var kickoff = match.KickoffInstant(zone);
            var untilKickoff = kickoff - now;

            foreach (var (label, before) in Windows)
            {
                var key = $"{match.Id}:{label}";
                // Fire when we are inside the window: between (before - poll) and before until kickoff.
                if (untilKickoff <= before && untilKickoff > before - PollInterval && !_sent.Contains(key))
                {
                    var recipients = match.Participants
                        .Where(p => p.Status is ParticipantStatus.Accepted or ParticipantStatus.WaitingList)
                        .Select(p => p.UserId)
                        .ToList();

                    await notifications.NotifyManyAsync(
                        recipients,
                        NotificationCategory.Match,
                        "Match reminder",
                        $"\"{match.Title}\" starts in about {label}.",
                        match.Id,
                        cancellationToken);

                    _sent.Add(key);
                    anySent = true;
                }
            }
        }

        if (anySent)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
