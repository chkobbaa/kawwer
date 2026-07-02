using CommunityToolkit.Mvvm.ComponentModel;

namespace Kawwer.Mobile.ViewModels;

/// <summary>Base for all view models. Provides busy state and a safe execution wrapper.</summary>
public abstract partial class BaseViewModel : ObservableObject
{
    private DateTime _lastLoadedUtc;

    [ObservableProperty]
    private bool _isBusy;

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
        try
        {
            await action();
            _lastLoadedUtc = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }
}
