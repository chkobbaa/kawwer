using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Kawwer.Mobile.Views;

namespace Kawwer.Mobile.Services;

/// <summary>
/// Central place for user-facing dialogs. Success messages use a styled, auto-dismissing
/// <see cref="SuccessPopup"/> instead of a native alert; confirmations and errors still use the
/// platform dialog because those genuinely need to block on a decision.
/// </summary>
public interface IDialogService
{
    Task ShowSuccessAsync(string message);
    Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);
    Task ShowAlertAsync(string title, string message, string cancel = "OK");
}

public sealed class DialogService : IDialogService
{
    private static readonly TimeSpan AutoDismissAfter = TimeSpan.FromSeconds(1.6);

    public Task ShowSuccessAsync(string message) => MainThread.InvokeOnMainThreadAsync(async () =>
    {
        var page = CurrentPage();
        if (page is null)
        {
            return;
        }

        var popup = new SuccessPopup(message);
        page.ShowPopup(popup, new PopupOptions { CanBeDismissedByTappingOutsideOfPopup = true });

        await Task.Delay(AutoDismissAfter);
        try
        {
            await popup.CloseAsync();
        }
        catch
        {
            // The user may have already dismissed it by tapping outside.
        }
    });

    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
        => MainThread.InvokeOnMainThreadAsync(() =>
        {
            var page = CurrentPage();
            return page is null ? Task.FromResult(false) : page.DisplayAlertAsync(title, message, accept, cancel);
        });

    public Task ShowAlertAsync(string title, string message, string cancel = "OK")
        => MainThread.InvokeOnMainThreadAsync(() =>
        {
            var page = CurrentPage();
            return page is null ? Task.CompletedTask : page.DisplayAlertAsync(title, message, cancel);
        });

    private static Page? CurrentPage()
        => Shell.Current?.CurrentPage ?? Application.Current?.Windows.FirstOrDefault()?.Page;
}
