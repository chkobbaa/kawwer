using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

[QueryProperty(nameof(MatchIdQuery), "matchId")]
public sealed partial class PaymentsViewModel : BaseViewModel
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
    private readonly RealtimeService _realtime;

    public PaymentsViewModel(KawwerApiClient api, RealtimeService realtime)
    {
        _api = api;
        _realtime = realtime;
        Title = "Payments";
    }

    public ObservableCollection<PaymentPlayerDto> Players { get; } = new();

    [ObservableProperty] private Guid _matchId;
    [ObservableProperty] private PaymentSummaryDto? _summary;

    partial void OnMatchIdChanged(Guid value) => _ = LoadAsync();

    /// <summary>Watch this match so the ledger updates the instant a payment is recorded.</summary>
    public void SubscribeRealtime()
    {
        _realtime.PaymentUpdated += OnPaymentChanged;
        _ = _realtime.JoinMatchAsync(MatchId);
    }

    public void UnsubscribeRealtime()
    {
        _realtime.PaymentUpdated -= OnPaymentChanged;
        _ = _realtime.LeaveMatchAsync(MatchId);
    }

    private void OnPaymentChanged(Guid matchId)
    {
        if (matchId == MatchId)
        {
            LoadCommand.Execute(null);
        }
    }

    [RelayCommand]
    public Task LoadAsync() => RunAsync(LoadCoreAsync);

    // Unguarded core so mutations (start/mark paid/finish) can reload while RunAsync holds IsBusy.
    private async Task LoadCoreAsync()
    {
        if (MatchId == Guid.Empty)
        {
            return;
        }

        Summary = await _api.GetPaymentSummaryAsync(MatchId);
        Players.Clear();
        foreach (var p in Summary.Players)
        {
            Players.Add(p);
        }
    }

    [RelayCommand]
    private Task StartAsync() => RunAsync(async () =>
    {
        await _api.StartCollectionAsync(MatchId);
        await LoadCoreAsync();
    });

    [RelayCommand]
    private Task MarkPaidAsync(Guid userId) => RunAsync(async () =>
    {
        await _api.MarkPaidAsync(MatchId, userId);
        await LoadCoreAsync();
    });

    [RelayCommand]
    private Task FinishAsync() => RunAsync(async () =>
    {
        await _api.FinishCollectionAsync(MatchId);
        await LoadCoreAsync();
    });
}
