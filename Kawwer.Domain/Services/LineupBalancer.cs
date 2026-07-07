using Kawwer.Domain.Enums;

namespace Kawwer.Domain.Services;

/// <summary>
/// A candidate for the auto-balancer. Carries only the signals that matter for fairness: an
/// optional 1..5 skill level and a 0..100 reputation. Guests (who have no reputation) are passed in
/// with <see cref="LineupBalancer.NeutralReputation"/>.
/// </summary>
public readonly record struct BalanceCandidate(int? SkillLevel, decimal Reputation);

/// <summary>The balancer's decision for one candidate, referenced back by its input index.</summary>
public readonly record struct LineupPlacement(int Index, TeamSide Team, double PositionX, double PositionY);

/// <summary>
/// Splits a set of players into two balanced teams and hands each a sensible starting position on
/// the pitch. The split uses the classic greedy number-partitioning heuristic: sort players by a
/// combined skill/reputation score (strongest first), then hand each to whichever team currently has
/// the lower total score. This keeps the two team totals as close as the data allows in a single
/// pass, and is deterministic so the same roster always balances the same way.
///
/// The class is intentionally pure and free of infrastructure so it can be unit-tested directly.
/// </summary>
public static class LineupBalancer
{
    /// <summary>Skill assumed for a player who has never set one (the middle of the 1..5 scale).</summary>
    public const int DefaultSkillLevel = 3;

    /// <summary>Reputation assumed for guests, who carry no reputation of their own (the midpoint).</summary>
    public const decimal NeutralReputation = 50m;

    /// <summary>
    /// A player's balance weight. Skill (1..5) and reputation (0..100, scaled to the same 0..5 band)
    /// contribute equally, so a highly-rated regular and a top-skill newcomer pull comparable weight.
    /// </summary>
    public static double ScoreOf(BalanceCandidate candidate)
    {
        var skill = candidate.SkillLevel ?? DefaultSkillLevel;
        return skill + (double)candidate.Reputation / 20d;
    }

    public static IReadOnlyList<LineupPlacement> Balance(IReadOnlyList<BalanceCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        // Strongest first; ties broken by input order so the result is deterministic.
        var ordered = Enumerable.Range(0, candidates.Count)
            .OrderByDescending(i => ScoreOf(candidates[i]))
            .ThenBy(i => i)
            .ToList();

        var teamA = new List<int>();
        var teamB = new List<int>();
        var totalA = 0d;
        var totalB = 0d;

        foreach (var index in ordered)
        {
            var score = ScoreOf(candidates[index]);

            // Feed the lighter team; on a tie feed the emptier one, then default to Team A.
            var chooseA = totalA < totalB
                          || (totalA == totalB && teamA.Count <= teamB.Count);

            if (chooseA)
            {
                teamA.Add(index);
                totalA += score;
            }
            else
            {
                teamB.Add(index);
                totalB += score;
            }
        }

        var placements = new List<LineupPlacement>(candidates.Count);
        AppendTeam(placements, teamA, TeamSide.TeamA);
        AppendTeam(placements, teamB, TeamSide.TeamB);
        return placements;
    }

    private static void AppendTeam(List<LineupPlacement> placements, IReadOnlyList<int> team, TeamSide side)
    {
        var formation = FormationPositions(team.Count);
        for (var i = 0; i < team.Count; i++)
        {
            placements.Add(new LineupPlacement(team[i], side, formation[i].X, formation[i].Y));
        }
    }

    /// <summary>
    /// A tidy default formation inside a team's own half, in normalized local coordinates where
    /// X runs from the goal line (0) to the halfway line (1) and Y runs across the width (0..1).
    /// One player anchors the goal; the rest fan out in a grid the organizer can then fine-tune.
    /// </summary>
    private static IReadOnlyList<(double X, double Y)> FormationPositions(int count)
    {
        var positions = new List<(double X, double Y)>(count);
        if (count <= 0)
        {
            return positions;
        }

        // First player keeps goal, centered near the goal line.
        positions.Add((0.10, 0.5));
        var outfield = count - 1;
        if (outfield == 0)
        {
            return positions;
        }

        // Fan the outfield players over a grid: columns are tactical lines (depth), rows spread
        // across the width. Up to four lines keeps even large squads readable.
        var columns = Math.Clamp((int)Math.Ceiling(Math.Sqrt(outfield)), 1, 4);
        var rows = (int)Math.Ceiling(outfield / (double)columns);

        for (var i = 0; i < outfield; i++)
        {
            var column = i % columns;
            var row = i / columns;

            var x = columns == 1 ? 0.55 : 0.30 + (0.95 - 0.30) * column / (columns - 1);
            var y = rows == 1 ? 0.5 : 0.15 + (0.85 - 0.15) * row / (rows - 1);
            positions.Add((x, y));
        }

        return positions;
    }
}
