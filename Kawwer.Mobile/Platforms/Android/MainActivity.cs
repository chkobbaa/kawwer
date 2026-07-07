using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Kawwer.Mobile.Services;

namespace kawwer;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    WindowSoftInputMode = SoftInput.AdjustResize,
    ScreenOrientation = ScreenOrientation.Portrait,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        ProcessNotificationIntent(Intent);
    }

    protected override void OnNewIntent(Android.Content.Intent? intent)
    {
        base.OnNewIntent(intent);
        ProcessNotificationIntent(intent);
    }

    /// <summary>
    /// A tapped push notification launches/resumes this activity with the notification's
    /// category and match id as extras; hand them to the Shell for deep-link navigation.
    /// </summary>
    private static void ProcessNotificationIntent(Android.Content.Intent? intent)
    {
        var category = intent?.GetStringExtra("category");
        var matchId = intent?.GetStringExtra("matchId");
        if (category is null && matchId is null)
        {
            return;
        }

        // Consume the extras so rotation/recreation doesn't re-trigger navigation.
        intent?.RemoveExtra("category");
        intent?.RemoveExtra("matchId");

        Kawwer.Mobile.Services.NotificationNavigation.SetPending(category, matchId);
    }

    // Swipe-to-switch-tab detection. Done at the activity level because MAUI's
    // SwipeGestureRecognizer never fires when the gesture starts over scrollable
    // content (CollectionView/ScrollView), which is nearly the whole screen.
    private float _downX;
    private float _downY;
    private long _downTime;

    public override bool DispatchTouchEvent(MotionEvent? e)
    {
        if (e is not null)
        {
            HideKeyboardOnOutsideTap(e);
            DetectTabSwipe(e);
        }

        return base.DispatchTouchEvent(e);
    }

    /// <summary>Tapping anywhere outside the focused text field dismisses the keyboard.</summary>
    private void HideKeyboardOnOutsideTap(MotionEvent e)
    {
        if (e.ActionMasked != MotionEventActions.Down || CurrentFocus is not EditText editText)
        {
            return;
        }

        var bounds = new Android.Graphics.Rect();
        editText.GetGlobalVisibleRect(bounds);
        if (bounds.Contains((int)e.RawX, (int)e.RawY))
        {
            return;
        }

        editText.ClearFocus();
        if (GetSystemService(InputMethodService) is InputMethodManager imm)
        {
            imm.HideSoftInputFromWindow(editText.WindowToken, HideSoftInputFlags.None);
        }
    }

    private void DetectTabSwipe(MotionEvent e)
    {
        switch (e.ActionMasked)
        {
            case MotionEventActions.Down:
                _downX = e.RawX;
                _downY = e.RawY;
                _downTime = e.EventTime;
                break;

            case MotionEventActions.Up:
                var dx = e.RawX - _downX;
                var dy = e.RawY - _downY;
                var elapsed = e.EventTime - _downTime;
                var density = Resources?.DisplayMetrics?.Density ?? 1f;

                // Quick, mostly-horizontal fling: at least 90dp sideways, twice the vertical travel.
                if (elapsed < 500 && Math.Abs(dx) > 90 * density && Math.Abs(dx) > Math.Abs(dy) * 2)
                {
                    var direction = dx < 0 ? SwipeDirection.Left : SwipeDirection.Right;
                    MainThread.BeginInvokeOnMainThread(() => TabSwipe.Handle(direction));
                }

                break;
        }
    }
}
