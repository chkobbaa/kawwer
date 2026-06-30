using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class NotificationsViewModel : BaseViewModel
{
    private readonly KawwerApiClient _api;

    public NotificationsViewModel(KawwerApiClient api)
    {
        _api = api;
        Title = "Notifications";
    }

    public ObservableCollection<NotificationDto> Notifications { get; } = new();

    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
    {
        var result = await _api.GetNotificationsAsync();
        Notifications.Clear();
        foreach (var n in result.Items)
        {
            Notifications.Add(n);
        }
    });

    [RelayCommand]
    private Task MarkReadAsync(Guid id) => RunAsync(async () =>
    {
        await _api.MarkNotificationReadAsync(id);
        await LoadAsync();
    });

    [RelayCommand]
    private Task MarkAllReadAsync() => RunAsync(async () =>
    {
        await _api.MarkAllReadAsync();
        await LoadAsync();
    });
}
