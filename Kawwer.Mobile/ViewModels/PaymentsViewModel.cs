using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

[QueryProperty(nameof(MatchId), "matchId")]
public sealed partial class PaymentsViewModel : BaseViewModel
{
    private readonly KawwerApiClient _api;

    public PaymentsViewModel(KawwerApiClient api)
    {
        _api = api;
        Title = "Payments";
    }

    public ObservableCollection<PaymentPlayerDto> Players { get; } = new();

    [ObservableProperty] private Guid _matchId;
    [ObservableProperty] private PaymentSummaryDto? _summary;

    partial void OnMatchIdChanged(Guid value) => _ = LoadAsync();

    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
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
    });

    [RelayCommand]
    private Task StartAsync() => RunAsync(async () =>
    {
        await _api.StartCollectionAsync(MatchId);
        await LoadAsync();
    });

    [RelayCommand]
    private Task MarkPaidAsync(Guid userId) => RunAsync(async () =>
    {
        await _api.MarkPaidAsync(MatchId, userId);
        await LoadAsync();
    });

    [RelayCommand]
    private Task FinishAsync() => RunAsync(async () =>
    {
        await _api.FinishCollectionAsync(MatchId);
        await LoadAsync();
    });
}
