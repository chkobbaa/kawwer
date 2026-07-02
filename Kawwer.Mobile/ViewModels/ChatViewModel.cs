using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

[QueryProperty(nameof(MatchIdQuery), "matchId")]
public sealed partial class ChatViewModel : BaseViewModel
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

    public ChatViewModel(KawwerApiClient api, SessionState session)
    {
        _api = api;
        _session = session;
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
            m.IsMine = m.SenderId is { } sender && sender == _session.UserId;
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
        message.IsMine = true;
        Messages.Add(message);
        Draft = string.Empty;
    });
}
