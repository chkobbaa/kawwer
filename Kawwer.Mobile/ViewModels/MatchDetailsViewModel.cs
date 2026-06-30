using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

[QueryProperty(nameof(MatchId), "matchId")]
public sealed partial class MatchDetailsViewModel : BaseViewModel
{
    private readonly KawwerApiClient _api;
    private readonly SessionState _session;

    public MatchDetailsViewModel(KawwerApiClient api, SessionState session)
    {
        _api = api;
        _session = session;
        Title = "Match";
    }

    public ObservableCollection<MatchParticipantDto> Participants { get; } = new();

    [ObservableProperty] private Guid _matchId;
    [ObservableProperty] private MatchDto? _match;
    [ObservableProperty] private bool _isOrganizer;
    [ObservableProperty] private MatchParticipantDto? _me;

    partial void OnMatchIdChanged(Guid value) => _ = LoadAsync();

    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
    {
        if (MatchId == Guid.Empty)
        {
            return;
        }

        Match = await _api.GetMatchAsync(MatchId);
        IsOrganizer = _session.UserId == Match.OrganizerId;

        var participants = await _api.GetParticipantsAsync(MatchId);
        Participants.Clear();
        foreach (var p in participants)
        {
            Participants.Add(p);
        }

        Me = participants.FirstOrDefault(p => p.User.Id == _session.UserId);
    });

    [RelayCommand]
    private Task AcceptAsync() => RunAsync(async () =>
    {
        await _api.RespondAsync(MatchId, true);
        await LoadAsync();
    });

    [RelayCommand]
    private Task DeclineAsync() => RunAsync(async () =>
    {
        await _api.RespondAsync(MatchId, false);
        await LoadAsync();
    });

    [RelayCommand]
    private Task LeaveAsync() => RunAsync(async () =>
    {
        await _api.LeaveAsync(MatchId);
        await LoadAsync();
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
                await LoadAsync();
            });
        }
    }

    [RelayCommand]
    private Task StartLiveAsync() => RunAsync(async () =>
    {
        await _api.StartLiveAsync(MatchId);
        await LoadAsync();
    });

    [RelayCommand]
    private Task FinishAsync() => RunAsync(async () =>
    {
        await _api.FinishAsync(MatchId);
        await LoadAsync();
    });

    [RelayCommand]
    private Task OpenChatAsync() => Shell.Current.GoToAsync($"chat?matchId={MatchId}");

    [RelayCommand]
    private Task OpenPaymentsAsync() => Shell.Current.GoToAsync($"payments?matchId={MatchId}");
}
