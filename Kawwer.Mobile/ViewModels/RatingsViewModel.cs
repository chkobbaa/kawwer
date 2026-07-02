using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

[QueryProperty(nameof(MatchIdQuery), "matchId")]
public sealed partial class RatingsViewModel : BaseViewModel
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

    public RatingsViewModel(KawwerApiClient api, SessionState session)
    {
        _api = api;
        _session = session;
        Title = "Rate the match";
    }

    public ObservableCollection<RatingItem> Items { get; } = new();

    [ObservableProperty] private Guid _matchId;

    partial void OnMatchIdChanged(Guid value) => _ = LoadAsync();

    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
    {
        if (MatchId == Guid.Empty)
        {
            return;
        }

        var match = await _api.GetMatchAsync(MatchId);
        var participants = await _api.GetParticipantsAsync(MatchId);

        Items.Clear();
        foreach (var p in participants.Where(p => p.Status == ParticipantStatus.Accepted && p.User.Id != _session.UserId))
        {
            var isOrganizer = p.User.Id == match.OrganizerId;
            Items.Add(new RatingItem(p.User, isOrganizer ? RatingType.Organizer : RatingType.Player));
        }
    });

    [RelayCommand]
    private Task SubmitAsync() => RunAsync(async () =>
    {
        var ratings = Items
            .Where(i => i.Stars > 0)
            .Select(i => new { rateeId = i.User.Id, type = i.Type, stars = i.Stars, comment = (string?)null })
            .ToList();

        if (ratings.Count == 0)
        {
            ErrorMessage = "Rate at least one player before submitting.";
            return;
        }

        await _api.SubmitRatingsAsync(MatchId, new { ratings });
        await Shell.Current.DisplayAlertAsync("Thank you", "Your ratings were submitted anonymously.", "OK");
        await Shell.Current.GoToAsync("..");
    });
}

/// <summary>One player to rate, with a 0-5 star selection.</summary>
public sealed partial class RatingItem : ObservableObject
{
    public RatingItem(UserSummaryDto user, RatingType type)
    {
        User = user;
        Type = type;
    }

    public UserSummaryDto User { get; }
    public RatingType Type { get; }
    public string RoleLabel => Type == RatingType.Organizer ? "Organizer" : "Player";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StarsLabel))]
    private int _stars;

    public string StarsLabel => Stars == 0 ? "Not rated" : $"{Stars}/5";

    [RelayCommand]
    private void SetStars(string value) => Stars = int.Parse(value);
}
