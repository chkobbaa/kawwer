using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

[QueryProperty(nameof(MatchId), "matchId")]
public sealed partial class ChatViewModel : BaseViewModel
{
    private readonly KawwerApiClient _api;

    public ChatViewModel(KawwerApiClient api)
    {
        _api = api;
        Title = "Match chat";
    }

    public ObservableCollection<ChatMessageDto> Messages { get; } = new();

    [ObservableProperty] private Guid _matchId;
    [ObservableProperty] private string _draft = string.Empty;

    partial void OnMatchIdChanged(Guid value) => _ = LoadAsync();

    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
    {
        if (MatchId == Guid.Empty)
        {
            return;
        }

        var result = await _api.GetMessagesAsync(MatchId);
        Messages.Clear();
        foreach (var m in result.Items)
        {
            Messages.Add(m);
        }
    });

    [RelayCommand]
    private Task SendAsync() => RunAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(Draft))
        {
            return;
        }

        var message = await _api.SendMessageAsync(MatchId, Draft.Trim());
        Messages.Add(message);
        Draft = string.Empty;
    });
}
