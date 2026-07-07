using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

/// <summary>
/// Drives the 2D tactical lineup board. Positions are normalized 0..1 within each token's own team
/// half. The board renders in two ways: a vertical half-pitch (one team at a time, with a toggle) and
/// a simulated-landscape full pitch drawn with a UI rotation transform — the OS orientation never
/// changes. Only the organizer can arrange the board.
/// </summary>
[QueryProperty(nameof(MatchIdQuery), "matchId")]
public sealed partial class LineupViewModel : BaseViewModel
{
    public string MatchIdQuery
    {
        set
        {
            if (Guid.TryParse(value, out var id))
            {
                MatchId = id;
            }
        }
    }

    private readonly KawwerApiClient _api;
    private readonly SessionState _session;
    private readonly RealtimeService _realtime;

    public LineupViewModel(KawwerApiClient api, SessionState session, RealtimeService realtime)
    {
        _api = api;
        _session = session;
        _realtime = realtime;
        Title = "Lineup";
    }

    /// <summary>Raised whenever the board's contents or view mode change, so the page can re-render.</summary>
    public event Action? BoardChanged;

    /// <summary>Every person who can be placed on the board (organizer, accepted players, guests).</summary>
    public ObservableCollection<LineupToken> Tokens { get; } = new();

    /// <summary>Players not on the team currently being viewed — the "bench" to pull from.</summary>
    public ObservableCollection<LineupToken> Bench { get; } = new();

    [ObservableProperty] private Guid _matchId;
    [ObservableProperty] private bool _isOrganizer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ViewingTeamName))]
    [NotifyPropertyChangedFor(nameof(ViewingTeamIsA))]
    private TeamSide _viewingTeam = TeamSide.TeamA;

    [ObservableProperty] private bool _showFullPitch;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountsLabel))]
    private int _teamACount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountsLabel))]
    private int _teamBCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBench))]
    [NotifyPropertyChangedFor(nameof(BenchLabel))]
    private int _benchCount;

    public string ViewingTeamName => ViewingTeam == TeamSide.TeamB ? "TEAM B" : "TEAM A";
    public bool ViewingTeamIsA => ViewingTeam != TeamSide.TeamB;
    public string CountsLabel => $"Team A · {TeamACount}    Team B · {TeamBCount}";
    public bool HasBench => BenchCount > 0;
    public string BenchLabel => $"AVAILABLE · {BenchCount}";

    public void SubscribeRealtime()
    {
        _realtime.MatchUpdated += OnMatchChanged;
        _ = _realtime.JoinMatchAsync(MatchId);
    }

    public void UnsubscribeRealtime()
    {
        _realtime.MatchUpdated -= OnMatchChanged;
        _ = _realtime.LeaveMatchAsync(MatchId);
    }

    private void OnMatchChanged(Guid matchId)
    {
        if (matchId == MatchId)
        {
            LoadCommand.Execute(null);
        }
    }

    partial void OnMatchIdChanged(Guid value) => _ = LoadAsync();

    partial void OnViewingTeamChanged(TeamSide value) => RefreshBuckets();

    partial void OnShowFullPitchChanged(bool value) => BoardChanged?.Invoke();

    [RelayCommand]
    public Task LoadAsync() => RunAsync(LoadCoreAsync);

    private async Task LoadCoreAsync()
    {
        if (MatchId == Guid.Empty)
        {
            return;
        }

        var lineup = await _api.GetLineupAsync(MatchId);
        Tokens.Clear();
        foreach (var slot in lineup.Slots)
        {
            Tokens.Add(new LineupToken(slot));
        }

        IsOrganizer = lineup.Slots.Any(s => s.Kind == LineupSlotKind.Organizer && s.Id == _session.UserId);
        RefreshBuckets();
    }

    /// <summary>Recomputes team counts and the bench, then asks the page to re-render.</summary>
    private void RefreshBuckets()
    {
        TeamACount = Tokens.Count(t => t.Team == TeamSide.TeamA);
        TeamBCount = Tokens.Count(t => t.Team == TeamSide.TeamB);

        Bench.Clear();
        foreach (var token in Tokens.Where(t => t.Team != ViewingTeam).OrderBy(t => t.ShortName))
        {
            Bench.Add(token);
        }

        BenchCount = Bench.Count;
        BoardChanged?.Invoke();
    }

    [RelayCommand]
    private Task AutoBalanceAsync() => RunAsync(async () =>
    {
        var lineup = await _api.AutoBalanceLineupAsync(MatchId);
        Tokens.Clear();
        foreach (var slot in lineup.Slots)
        {
            Tokens.Add(new LineupToken(slot));
        }

        RefreshBuckets();
        await Dialog.ShowSuccessAsync("Teams balanced by skill & reputation. Drag players to fine-tune.");
    });

    [RelayCommand]
    private void ToggleFullPitch() => ShowFullPitch = !ShowFullPitch;

    [RelayCommand]
    private void ShowTeamA() => ViewingTeam = TeamSide.TeamA;

    [RelayCommand]
    private void ShowTeamB() => ViewingTeam = TeamSide.TeamB;

    /// <summary>Pulls a benched player onto the team currently being viewed at a tidy free spot.</summary>
    [RelayCommand]
    private async Task AssignToViewingTeamAsync(LineupToken token)
    {
        if (token is null || !IsOrganizer)
        {
            return;
        }

        var index = Tokens.Count(t => t.Team == ViewingTeam);
        token.Team = ViewingTeam;
        token.PositionX = 0.45 + 0.12 * (index / 5 % 3);
        token.PositionY = 0.15 + 0.70 * (index % 5) / 4.0;

        RefreshBuckets();
        await SaveSlotAsync(token);
    }

    /// <summary>Sends a placed player back to the bench (unassigned) without losing their spot on reload.</summary>
    public async Task SendToBenchAsync(LineupToken token)
    {
        if (token is null || !IsOrganizer)
        {
            return;
        }

        token.Team = TeamSide.Unassigned;
        RefreshBuckets();
        await SaveSlotAsync(token);
    }

    /// <summary>Persists one token's team + position. Called after a drag or a bench move.</summary>
    public async Task SaveSlotAsync(LineupToken token)
    {
        if (token is null || !IsOrganizer)
        {
            return;
        }

        try
        {
            await _api.UpdateLineupSlotAsync(
                MatchId, token.Kind, token.Id, token.Team, token.PositionX, token.PositionY);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
