using System.Collections.Concurrent;

namespace Kawwer.Api.Logging;

/// <summary>A single captured log entry, in a shape that serialises cleanly for the logs viewer.</summary>
public sealed record LogEntry(
    long Sequence,
    DateTime TimestampUtc,
    string Level,
    string Category,
    string Message,
    string? Exception);

/// <summary>
/// A bounded, thread-safe ring buffer of the most recent log entries. Kept in memory (no files,
/// no external sink) so the password-gated logs page can read recent server activity from any
/// device without extra infrastructure. Oldest entries are dropped once <see cref="Capacity"/> is
/// reached.
/// </summary>
public sealed class InMemoryLogStore
{
    public const int Capacity = 2000;

    private readonly ConcurrentQueue<LogEntry> _entries = new();
    private long _sequence;

    public void Add(DateTime timestampUtc, string level, string category, string message, string? exception)
    {
        var entry = new LogEntry(
            Interlocked.Increment(ref _sequence),
            timestampUtc,
            level,
            category,
            message,
            exception);

        _entries.Enqueue(entry);

        while (_entries.Count > Capacity && _entries.TryDequeue(out _))
        {
            // Trim from the front so memory stays bounded.
        }
    }

    /// <summary>
    /// Returns recent entries, newest last. Optionally filters to a minimum level and to entries
    /// after a given sequence number (so the viewer can poll for just what's new).
    /// </summary>
    public IReadOnlyList<LogEntry> GetRecent(int limit, string? minLevel, long afterSequence)
    {
        var minRank = LevelRank(minLevel);
        var query = _entries
            .Where(e => e.Sequence > afterSequence && LevelRank(e.Level) >= minRank)
            .OrderBy(e => e.Sequence);

        var list = query.ToList();
        if (limit > 0 && list.Count > limit)
        {
            list = list.Skip(list.Count - limit).ToList();
        }

        return list;
    }

    private static int LevelRank(string? level) => level?.ToLowerInvariant() switch
    {
        "trace" => 0,
        "debug" => 1,
        "information" or "info" => 2,
        "warning" or "warn" => 3,
        "error" => 4,
        "critical" or "fatal" => 5,
        "none" => 6,
        _ => 0
    };
}
