namespace Kawwer.Mobile.Views;

/// <summary>
/// A full-screen "incoming call" overlay used by <see cref="Services.CallSimulationService"/> to
/// make important updates (e.g. a match reschedule) hard to ignore when the user is in Call mode.
/// It rings with a pulsing badge, auto-dismisses after a few seconds, and lets the user dismiss or
/// open it early. Either way, the normal in-app notification remains in their activity feed.
/// </summary>
public partial class IncomingCallPage : ContentPage
{
    private readonly TaskCompletionSource _dismissed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _completed;
    private CancellationTokenSource? _pulseCts;

    public IncomingCallPage(string caller, string detail)
    {
        InitializeComponent();
        CallerLabel.Text = caller;
        DetailLabel.Text = detail;
    }

    /// <summary>Completes when the call screen has been dismissed (by the user or the auto-timeout).</summary>
    public Task Dismissed => _dismissed.Task;

    public bool IsDismissed => _completed;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _pulseCts = new CancellationTokenSource();
        _ = PulseAsync(_pulseCts.Token);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _pulseCts?.Cancel();
        Complete();
    }

    private async Task PulseAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await Pulse.ScaleTo(1.08, 500, Easing.SinInOut);
                await Pulse.ScaleTo(1.0, 500, Easing.SinInOut);
            }
        }
        catch
        {
            // Animation is cosmetic.
        }
    }

    private async void OnDismiss(object? sender, EventArgs e)
    {
        Complete();
        await SafePopAsync();
    }

    private async void OnView(object? sender, EventArgs e)
    {
        Complete();
        await SafePopAsync();
        try
        {
            await Shell.Current.GoToAsync("notifications");
        }
        catch
        {
            // Best effort: the notification is already in the feed.
        }
    }

    protected override bool OnBackButtonPressed()
    {
        Complete();
        return base.OnBackButtonPressed();
    }

    private async Task SafePopAsync()
    {
        try
        {
            if (Navigation.ModalStack.Count > 0)
            {
                await Navigation.PopModalAsync();
            }
        }
        catch
        {
            // Already dismissed elsewhere.
        }
    }

    private void Complete()
    {
        if (!_completed)
        {
            _completed = true;
            _dismissed.TrySetResult();
        }
    }
}
