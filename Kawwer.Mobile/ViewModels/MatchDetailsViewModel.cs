using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

[QueryProperty(nameof(MatchIdQuery), "matchId")]
public sealed partial class MatchDetailsViewModel : BaseViewModel
{
    /// <summary>Shell query values arrive as strings; parse instead of casting to Guid.</summary>
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

    public MatchDetailsViewModel(KawwerApiClient api, SessionState session)
    {
        _api = api;
        _session = session;
        Title = "Match";
    }

    public ObservableCollection<MatchParticipantDto> Participants { get; } = new();

    /// <summary>Players with a confirmed spot.</summary>
    public ObservableCollection<MatchParticipantDto> Players { get; } = new();

    /// <summary>Players queued on the waiting list, in order.</summary>
    public ObservableCollection<MatchParticipantDto> Waitlist { get; } = new();

    /// <summary>Pending invitations and join requests (shown to the organizer).</summary>
    public ObservableCollection<MatchParticipantDto> Pending { get; } = new();

    [ObservableProperty] private Guid _matchId;
    [ObservableProperty] private MatchDto? _match;
    [ObservableProperty] private bool _isOrganizer;
    [ObservableProperty] private MatchParticipantDto? _me;
    [ObservableProperty] private bool _canRespond;
    [ObservableProperty] private bool _canLeave;
    [ObservableProperty] private bool _canJoin;
    [ObservableProperty] private bool _isLive;
    [ObservableProperty] private bool _isFinished;
    [ObservableProperty] private bool _isCancelled;
    [ObservableProperty] private bool _showOrganizerActions;
    [ObservableProperty] private bool _hasWaitlist;
    [ObservableProperty] private bool _hasPending;
    [ObservableProperty] private WaitingListPositionDto? _waitingPosition;
    [ObservableProperty] private bool _canInvite;
    [ObservableProperty] private string _inviteButtonText = "Invite players";

    partial void OnMatchIdChanged(Guid value) => _ = LoadAsync();

    [RelayCommand]
    public Task LoadAsync() => RunAsync(LoadCoreAsync);

    // Unguarded core so mutations (accept/leave/cancel/...) can reload while RunAsync holds IsBusy.
    private async Task LoadCoreAsync()
    {
        if (MatchId == Guid.Empty)
        {
            return;
        }

        Match = await _api.GetMatchAsync(MatchId);
        IsOrganizer = _session.UserId == Match.OrganizerId;
        IsCancelled = Match.Status == MatchStatus.Cancelled;
        IsFinished = Match.Status == MatchStatus.Finished;
        var isClosed = IsCancelled || IsFinished;

        var participants = await _api.GetParticipantsAsync(MatchId);
        Participants.Clear();
        Players.Clear();
        Waitlist.Clear();
        Pending.Clear();
        foreach (var p in participants)
        {
            // The organizer can approve or reject pending public join requests inline.
            p.CanRespondToJoinRequest = IsOrganizer && p.IsPendingJoinRequest && !isClosed;
            Participants.Add(p);

            switch (p.Status)
            {
                case ParticipantStatus.Accepted:
                    Players.Add(p);
                    break;
                case ParticipantStatus.WaitingList:
                    Waitlist.Add(p);
                    break;
                case ParticipantStatus.Invited or ParticipantStatus.Seen or ParticipantStatus.Thinking:
                    Pending.Add(p);
                    break;
            }
        }

        HasWaitlist = Waitlist.Count > 0;
        HasPending = IsOrganizer && Pending.Count > 0;

        Me = participants.FirstOrDefault(p => p.User.Id == _session.UserId);

        // Only show the actions that make sense for the viewer's current state.
        // A cancelled or finished match accepts no responses of any kind.
        CanRespond = !isClosed
                     && Me is { Status: ParticipantStatus.Invited or ParticipantStatus.Seen or ParticipantStatus.Thinking };
        CanLeave = !isClosed
                   && Me is { Status: ParticipantStatus.Accepted or ParticipantStatus.WaitingList };

        // Viewers discovering a public (or friends-only) match can ask to join from here.
        CanJoin = !isClosed
                  && !IsOrganizer
                  && Match.Visibility != MatchVisibility.Private
                  && (Me is null or { Status: ParticipantStatus.Declined or ParticipantStatus.Cancelled or ParticipantStatus.Removed });

        ShowOrganizerActions = IsOrganizer && !isClosed;
        IsLive = Match.LiveMatchStarted && !isClosed;

        // Inviting stays available after the match is created: the organizer invites
        // directly, while regular members suggest players the organizer must confirm.
        CanInvite = !isClosed && (IsOrganizer || Me is { Status: ParticipantStatus.Accepted });
        InviteButtonText = IsOrganizer ? "Invite players" : "Suggest players";

        // Waiting list details for the viewer (docs/WaitingList.md).
        WaitingPosition = null;
        if (Me is { Status: ParticipantStatus.WaitingList })
        {
            try
            {
                WaitingPosition = await _api.GetWaitingListPositionAsync(MatchId);
            }
            catch
            {
                // Position is informative only; the page still works without it.
            }
        }
    }

    /// <summary>Request to join this public match (auto-accept joins immediately).</summary>
    [RelayCommand]
    private Task JoinAsync() => RunAsync(async () =>
    {
        var joined = await _api.JoinPublicMatchAsync(MatchId);
        await Shell.Current.DisplayAlertAsync(
            "Join match",
            joined
                ? "You're in! The match is now in your calendar."
                : "The match is full or needs the organizer's approval. You're in the queue and will be notified.",
            "OK");
        await LoadCoreAsync();
    });

    /// <summary>Opens the profile of any player or the organizer.</summary>
    [RelayCommand]
    private Task OpenProfileAsync(Guid userId) => Shell.Current.GoToAsync($"playerprofile?userId={userId}");

    /// <summary>Opens the field location in the device's maps app.</summary>
    [RelayCommand]
    private async Task OpenMapAsync()
    {
        if (Match?.Field is not { } field)
        {
            return;
        }

        var lat = field.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var lng = field.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var url = string.IsNullOrWhiteSpace(field.GoogleMapsUrl)
            ? $"https://www.google.com/maps/search/?api=1&query={lat},{lng}"
            : field.GoogleMapsUrl;

        try
        {
            await Launcher.Default.OpenAsync(new Uri(url));
        }
        catch
        {
            // No handler available; nothing else to do.
        }
    }

    [RelayCommand]
    private Task AcceptAsync() => RunAsync(async () =>
    {
        await _api.RespondAsync(MatchId, true);
        await LoadCoreAsync();
    });

    [RelayCommand]
    private Task DeclineAsync() => RunAsync(async () =>
    {
        await _api.RespondAsync(MatchId, false);
        await LoadCoreAsync();
    });

    [RelayCommand]
    private async Task LeaveAsync()
    {
        var confirm = await Shell.Current.DisplayAlertAsync(
            "Leave match",
            "Are you sure you want to leave this match? Your spot will go to the next player in line.",
            "Leave",
            "Stay");
        if (!confirm)
        {
            return;
        }

        await RunAsync(async () =>
        {
            await _api.LeaveAsync(MatchId);
            await LoadCoreAsync();
        });
    }

    /// <summary>Opens the invite/suggest players screen.</summary>
    [RelayCommand]
    private Task OpenInviteAsync() => Shell.Current.GoToAsync($"inviteplayers?matchId={MatchId}");

    [RelayCommand]
    private Task ApproveJoinAsync(Guid userId) => RunAsync(async () =>
    {
        await _api.ApproveJoinRequestAsync(MatchId, userId);
        await LoadCoreAsync();
    });

    [RelayCommand]
    private Task RejectJoinAsync(Guid userId) => RunAsync(async () =>
    {
        await _api.RejectJoinRequestAsync(MatchId, userId);
        await LoadCoreAsync();
    });

    [RelayCommand]
    private async Task CancelAsync()
    {
        var confirm = await Shell.Current.DisplayAlertAsync("Cancel match", "Are you sure? Everyone will be notified.", "Yes", "No");
        if (confirm)
        {
            await RunAsync(async () =>
            {
                await _api.CancelAsync(MatchId);
                await LoadCoreAsync();
            });
        }
    }

    [RelayCommand]
    private async Task StartLiveAsync()
    {
        await RunAsync(() => _api.StartLiveAsync(MatchId));
        await Shell.Current.GoToAsync($"livematch?matchId={MatchId}");
    }

    [RelayCommand]
    private Task OpenLiveAsync() => Shell.Current.GoToAsync($"livematch?matchId={MatchId}");

    [RelayCommand]
    private Task OpenRatingsAsync() => Shell.Current.GoToAsync($"ratings?matchId={MatchId}");

    [RelayCommand]
    private Task FinishAsync() => RunAsync(async () =>
    {
        await _api.FinishAsync(MatchId);
        await LoadCoreAsync();
    });

    [RelayCommand]
    private Task OpenChatAsync() => Shell.Current.GoToAsync($"chat?matchId={MatchId}");

    [RelayCommand]
    private Task OpenPaymentsAsync() => Shell.Current.GoToAsync($"payments?matchId={MatchId}");
}
