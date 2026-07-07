using Kawwer.Mobile.Models;
using Kawwer.Mobile.Views;

namespace Kawwer.Mobile.Services;

/// <summary>
/// Escalates important, easy-to-miss real-time updates into a simulated incoming "call" when the
/// user has switched delivery to Call mode (<see cref="DeliveryPreference"/>).
///
/// It listens to the app-wide user-scoped real-time stream. When an <see cref="RealtimeUserEvent"/>
/// flagged <c>Important</c> arrives (e.g. a match reschedule) and Call mode is on, it presents a
/// full-screen call screen for ~3 seconds. The normal notification still lands (the server always
/// sends it), so Call mode only *adds* a hard-to-ignore prompt — it never replaces the record.
///
/// Registered as an eager singleton so it stays subscribed for the whole app lifetime, regardless
/// of which screen is on top.
/// </summary>
public sealed class CallSimulationService
{
    private static readonly TimeSpan RingDuration = TimeSpan.FromSeconds(3);

    private readonly RealtimeService _realtime;
    private bool _presenting;

    public CallSimulationService(RealtimeService realtime)
    {
        _realtime = realtime;
        _realtime.UserEvent += OnUserEvent;
    }

    private void OnUserEvent(RealtimeUserEvent e)
    {
        if (!e.Important || !DeliveryPreference.CallMode)
        {
            return; // Not important, or the user prefers plain notifications.
        }

        // UserEvent is already raised on the UI thread, but guard anyway before touching navigation.
        MainThread.BeginInvokeOnMainThread(async () => await PresentAsync(e));
    }

    private async Task PresentAsync(RealtimeUserEvent e)
    {
        if (_presenting)
        {
            return; // One simulated call at a time.
        }

        var navigation = Shell.Current?.Navigation
                         ?? Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation;
        if (navigation is null)
        {
            return;
        }

        _presenting = true;
        var caller = string.IsNullOrWhiteSpace(e.Title) ? "Kawwer" : e.Title!;
        var detail = string.IsNullOrWhiteSpace(e.Message) ? "Important update" : e.Message!;
        var page = new IncomingCallPage(caller, detail);

        try
        {
            await navigation.PushModalAsync(page, animated: true);

            // Ring for a few seconds, unless the user taps to dismiss sooner.
            await Task.WhenAny(page.Dismissed, Task.Delay(RingDuration));

            if (!page.IsDismissed)
            {
                await navigation.PopModalAsync(animated: true);
            }
        }
        catch
        {
            // Never let a presentation glitch break the app; the normal notification still lands.
        }
        finally
        {
            _presenting = false;
        }
    }
}
