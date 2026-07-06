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
    private readonly RealtimeService _realtime;

    public ChatViewModel(KawwerApiClient api, SessionState session, RealtimeService realtime)
    {
        _api = api;
        _session = session;
        _realtime = realtime;
        Title = "Match chat";
    }

    public ObservableCollection<ChatMessageDto> Messages { get; } = new();

    [ObservableProperty] private Guid _matchId;
    [ObservableProperty] private string _draft = string.Empty;

    partial void OnMatchIdChanged(Guid value) => _ = LoadAsync();

    /// <summary>Watch this match so new messages appear instantly, live.</summary>
    public void SubscribeRealtime()
    {
        _realtime.ChatMessagePosted += OnChatMessagePosted;
        _ = _realtime.JoinMatchAsync(MatchId);
    }

    public void UnsubscribeRealtime()
    {
        _realtime.ChatMessagePosted -= OnChatMessagePosted;
        _ = _realtime.LeaveMatchAsync(MatchId);
    }

    private void OnChatMessagePosted(Guid matchId, ChatMessageDto message)
    {
        if (matchId != MatchId || Messages.Any(m => m.Id == message.Id))
        {
            // Not this chat, or a message we already added optimistically after sending it.
            return;
        }

        message.IsMine = message.SenderId is { } sender && sender == _session.UserId;
        Messages.Add(message);
    }

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
