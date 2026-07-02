using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

[QueryProperty(nameof(MatchIdQuery), "matchId")]
public sealed partial class LiveMatchViewModel : BaseViewModel
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

    public LiveMatchViewModel(KawwerApiClient api, SessionState session)
    {
        _api = api;
        _session = session;
        Title = "Live match";
    }

    public ObservableCollection<LivePlayerItem> Players { get; } = new();

    [ObservableProperty] private Guid _matchId;
    [ObservableProperty] private MatchDto? _match;
    [ObservableProperty] private bool _isOrganizer;
    [ObservableProperty] private bool _isSharingLocation;
    [ObservableProperty] private int _presentCount;
    [ObservableProperty] private int _totalCount;

    partial void OnMatchIdChanged(Guid value) => _ = LoadAsync();

    [RelayCommand]
    public Task LoadAsync() => RunAsync(LoadCoreAsync);

    // Unguarded core so mutations (attendance/location) can reload while RunAsync holds IsBusy.
    private async Task LoadCoreAsync()
    {
        if (MatchId == Guid.Empty)
        {
            return;
        }

        Match = await _api.GetMatchAsync(MatchId);
        IsOrganizer = _session.UserId == Match.OrganizerId;

        var participants = await _api.GetParticipantsAsync(MatchId);
        var accepted = participants.Where(p => p.Status == ParticipantStatus.Accepted).ToList();

        Players.Clear();
        foreach (var p in accepted)
        {
            Players.Add(new LivePlayerItem(p, IsOrganizer && p.User.Id != _session.UserId));
        }

        var me = accepted.FirstOrDefault(p => p.User.Id == _session.UserId);
        IsSharingLocation = me?.SharedLocation ?? false;
        PresentCount = accepted.Count(p => p.Attendance == AttendanceStatus.Present);
        TotalCount = accepted.Count;
    }

    [RelayCommand]
    private Task MarkArrivedAsync() => RunAsync(async () =>
    {
        await _api.UpdateAttendanceAsync(MatchId, _session.UserId!.Value, AttendanceStatus.Present);
        await LoadCoreAsync();
    });

    [RelayCommand]
    private Task ShareLocationAsync() => RunAsync(async () =>
    {
        var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            ErrorMessage = "Location permission is required to share where you are.";
            return;
        }

        var location = await Geolocation.Default.GetLocationAsync(
            new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));
        if (location is null)
        {
            ErrorMessage = "Could not read your location.";
            return;
        }

        await _api.ShareLocationAsync(MatchId, (decimal)location.Latitude, (decimal)location.Longitude);
        IsSharingLocation = true;
        await LoadCoreAsync();
    });

    [RelayCommand]
    private Task StopSharingAsync() => RunAsync(async () =>
    {
        await _api.StopSharingLocationAsync(MatchId);
        IsSharingLocation = false;
        await LoadCoreAsync();
    });

    [RelayCommand]
    private async Task OpenNavigationAsync()
    {
        if (Match is null)
        {
            return;
        }

        var url = $"https://www.google.com/maps/dir/?api=1&destination={Match.Field.Latitude},{Match.Field.Longitude}";
        try
        {
            await Launcher.Default.OpenAsync(url);
        }
        catch
        {
            ErrorMessage = "Could not open navigation on this device.";
        }
    }

    [RelayCommand]
    private Task RequestLocationsAsync() => RunAsync(async () =>
    {
        await _api.RequestLocationsAsync(MatchId);
        await Shell.Current.DisplayAlertAsync("Live match", "Players were asked to share their location.", "OK");
    });

    [RelayCommand]
    private Task SetAttendanceAsync(AttendanceParameter parameter) => RunAsync(async () =>
    {
        await _api.UpdateAttendanceAsync(MatchId, parameter.UserId, parameter.Status);
        await LoadCoreAsync();
    });

    [RelayCommand]
    private Task MarkPresentAsync(Guid userId) => SetAttendanceAsync(new AttendanceParameter(userId, AttendanceStatus.Present));

    [RelayCommand]
    private Task MarkLateAsync(Guid userId) => SetAttendanceAsync(new AttendanceParameter(userId, AttendanceStatus.Late));

    [RelayCommand]
    private Task MarkNoShowAsync(Guid userId) => SetAttendanceAsync(new AttendanceParameter(userId, AttendanceStatus.NoShow));

    [RelayCommand]
    private Task OpenChatAsync() => Shell.Current.GoToAsync($"chat?matchId={MatchId}");
}

public sealed record AttendanceParameter(Guid UserId, AttendanceStatus Status);

/// <summary>Row model for the live attendance board.</summary>
public sealed class LivePlayerItem
{
    public LivePlayerItem(MatchParticipantDto participant, bool canManage)
    {
        Participant = participant;
        CanManage = canManage;
    }

    public MatchParticipantDto Participant { get; }
    public bool CanManage { get; }
    public Guid UserId => Participant.User.Id;

    public string AttendanceLabel => Participant.Attendance switch
    {
        AttendanceStatus.Present => "Arrived",
        AttendanceStatus.Travelling => "On the way",
        AttendanceStatus.Late => "Late",
        AttendanceStatus.NoShow => "No-show",
        _ => "No update"
    };

    public bool IsPresent => Participant.Attendance == AttendanceStatus.Present;
    public bool IsProblem => Participant.Attendance is AttendanceStatus.Late or AttendanceStatus.NoShow;
    public bool HasSharedLocation => Participant.SharedLocation;
}
