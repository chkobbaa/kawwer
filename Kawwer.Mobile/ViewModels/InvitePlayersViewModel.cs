using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

/// <summary>
/// Invite (or suggest) players to an existing match. The organizer's invitations go out
/// directly; a regular member's picks are sent to the organizer as suggestions that need
/// their confirmation before the players are added. The organizer (or an accepted member) can
/// also add "guest" players by name — people who don't have the app but will show up in the
/// roster and on the tactical lineup board.
/// </summary>
[QueryProperty(nameof(MatchIdQuery), "matchId")]
public sealed partial class InvitePlayersViewModel : BaseViewModel
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

    public InvitePlayersViewModel(KawwerApiClient api, SessionState session)
    {
        _api = api;
        _session = session;
        Title = "Invite players";
    }

    public ObservableCollection<SelectableUser> Friends { get; } = new();

    /// <summary>Guest players already added to this match.</summary>
    public ObservableCollection<GuestPlayerDto> Guests { get; } = new();

    [ObservableProperty] private Guid _matchId;
    [ObservableProperty] private bool _isOrganizer;
    [ObservableProperty] private string _hint = string.Empty;
    [ObservableProperty] private string _sendButtonText = "Send invitations";

    /// <summary>Draft name for a new guest player.</summary>
    [ObservableProperty] private string _guestName = string.Empty;
    [ObservableProperty] private bool _hasGuests;

    partial void OnMatchIdChanged(Guid value) => _ = LoadAsync();

    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
    {
        if (MatchId == Guid.Empty)
        {
            return;
        }

        var match = await _api.GetMatchAsync(MatchId);
        IsOrganizer = _session.UserId == match.OrganizerId;
        Title = IsOrganizer ? "Invite players" : "Suggest players";
        Hint = IsOrganizer
            ? "Invited friends receive an invitation and can accept or decline."
            : "Your picks are sent to the organizer, who confirms them before they join.";
        SendButtonText = IsOrganizer ? "Send invitations" : "Send suggestions";

        // Friends who already have an active link to the match cannot be picked again.
        var participants = await _api.GetParticipantsAsync(MatchId);
        var active = participants
            .Where(p => p.Status is ParticipantStatus.Invited
                        or ParticipantStatus.Seen
                        or ParticipantStatus.Thinking
                        or ParticipantStatus.Accepted
                        or ParticipantStatus.WaitingList)
            .Select(p => p.User.Id)
            .ToHashSet();
        active.Add(match.OrganizerId);

        var friends = await _api.GetFriendsAsync();
        Friends.Clear();
        foreach (var friend in friends.Where(f => !active.Contains(f.User.Id)))
        {
            Friends.Add(new SelectableUser(friend.User));
        }

        await LoadGuestsAsync();
    });

    private async Task LoadGuestsAsync()
    {
        try
        {
            var lineup = await _api.GetLineupAsync(MatchId);
            Guests.Clear();
            foreach (var slot in lineup.Slots.Where(s => s.IsGuest).OrderBy(s => s.DisplayName))
            {
                Guests.Add(new GuestPlayerDto
                {
                    Id = slot.Id,
                    MatchId = MatchId,
                    Name = slot.DisplayName,
                    SkillLevel = slot.SkillLevel,
                    Team = slot.Team,
                    PositionX = slot.PositionX,
                    PositionY = slot.PositionY
                });
            }

            HasGuests = Guests.Count > 0;
        }
        catch
        {
            // The guest list is a nice-to-have on this screen; never block inviting because of it.
        }
    }

    [RelayCommand]
    private Task AddGuestAsync() => RunAsync(async () =>
    {
        var name = GuestName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            ErrorMessage = "Enter the guest's name.";
            return;
        }

        var guest = await _api.AddGuestAsync(MatchId, name, skillLevel: null);
        Guests.Add(guest);
        HasGuests = Guests.Count > 0;
        GuestName = string.Empty;
        ErrorMessage = string.Empty;
    });

    [RelayCommand]
    private Task RemoveGuestAsync(GuestPlayerDto guest) => RunAsync(async () =>
    {
        await _api.RemoveGuestAsync(MatchId, guest.Id);
        Guests.Remove(guest);
        HasGuests = Guests.Count > 0;
    });

    [RelayCommand]
    private Task SendAsync() => RunAsync(async () =>
    {
        var selected = Friends.Where(f => f.IsSelected).Select(f => f.User.Id).ToList();
        if (selected.Count == 0)
        {
            ErrorMessage = "Select at least one player.";
            return;
        }

        await _api.InviteAsync(MatchId, selected, Array.Empty<Guid>());

        await Shell.Current.DisplayAlertAsync(
            Title,
            IsOrganizer
                ? "Invitations sent."
                : "Suggestions sent. The organizer will confirm them.",
            "OK");
        await Shell.Current.GoToAsync("..");
    });
}
