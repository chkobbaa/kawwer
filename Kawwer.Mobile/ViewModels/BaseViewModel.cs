using CommunityToolkit.Mvvm.ComponentModel;
using Kawwer.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kawwer.Mobile.ViewModels;

/// <summary>Base for all view models. Provides busy state and a safe execution wrapper.</summary>
public abstract partial class BaseViewModel : ObservableObject
{
    private DateTime _lastLoadedUtc;

    /// <summary>
    /// Shared dialog/popup service, resolved from the app's service provider so subclasses can show
    /// styled success popups and confirmations without each having to inject it.
    /// </summary>
    protected static IDialogService Dialog =>
        IPlatformApplication.Current?.Services.GetRequiredService<IDialogService>()
        ?? throw new InvalidOperationException("The application service provider is not available yet.");

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowSkeleton))]
    private bool _isBusy;

    /// <summary>
    /// True until the first load completes. Combined with <see cref="IsBusy"/> it drives
    /// <see cref="ShowSkeleton"/> so the shimmering skeleton only appears on the very first fetch;
    /// later pull-to-refresh loads keep the existing content on screen.
    /// </summary>
    private bool _hasLoadedOnce;

    /// <summary>True on the initial load, so pages can show a skeleton placeholder instead of a spinner.</summary>
    public bool ShowSkeleton => IsBusy && !_hasLoadedOnce;

    /// <summary>
    /// Bound to RefreshView.IsRefreshing. Kept separate from IsBusy because RefreshView
    /// sets this to true through the two-way binding BEFORE the refresh command runs;
    /// binding it to IsBusy made RunAsync bail out early and the spinner never stopped.
    /// </summary>
    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// True when the last load failed. Lets a page show an inline "Retry" affordance instead of a
    /// blank screen. Cleared whenever a load starts or succeeds.
    /// </summary>
    [ObservableProperty]
    private bool _isErrorState;

    /// <summary>True when the last successful load is older than <paramref name="maxAge"/>.
    /// Lets pages skip reloading on every appearance, which keeps tab navigation snappy.</summary>
    public bool IsStale(TimeSpan maxAge) => DateTime.UtcNow - _lastLoadedUtc >= maxAge;

    protected async Task RunAsync(Func<Task> action)
    {
        if (IsBusy)
        {
            IsRefreshing = false;
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        IsErrorState = false;
        try
        {
            await action();
            _lastLoadedUtc = DateTime.UtcNow;
            // Set before IsBusy flips false in the finally block, so ShowSkeleton recomputes to
            // false (and stays there) once we have content.
            _hasLoadedOnce = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            IsErrorState = true;
        }
        finally
        {
            // Always release the spinners, even on failure, so the UI never gets stuck showing a
            // perpetual "loading" state (the root cause of the app appearing frozen after idle).
            IsBusy = false;
            IsRefreshing = false;
        }
    }
}
