using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Navigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace kawwer;

/// <summary>
/// Customizes the bottom tab bar:
/// - hides the tab labels (icon-only navigation),
/// - re-tapping the current tab pops any pushed subpage back to the tab's root
///   (e.g. tapping "Profile" while in Settings returns to the Profile page).
/// </summary>
public class KawwerShellRenderer : ShellRenderer
{
    protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
        => new KawwerBottomNavAppearanceTracker(this, shellItem);
}

internal sealed class KawwerBottomNavAppearanceTracker : ShellBottomNavViewAppearanceTracker
{
    public KawwerBottomNavAppearanceTracker(IShellContext shellContext, ShellItem shellItem)
        : base(shellContext, shellItem)
    {
    }

    public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
    {
        base.SetAppearance(bottomView, appearance);

        // Icons only; the icons are self-explanatory.
        bottomView.LabelVisibilityMode = NavigationBarView.LabelVisibilityUnlabeled;

        // SetAppearance can run multiple times; keep exactly one subscription.
        bottomView.ItemReselected -= OnItemReselected;
        bottomView.ItemReselected += OnItemReselected;
    }

    private void OnItemReselected(object? sender, NavigationBarView.ItemReselectedEventArgs e)
    {
        var shell = Shell.Current;
        if (shell?.Navigation?.NavigationStack is { Count: > 1 })
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await shell.Navigation.PopToRootAsync();
                }
                catch
                {
                    // Best effort; the back arrow still works.
                }
            });
        }
    }
}
