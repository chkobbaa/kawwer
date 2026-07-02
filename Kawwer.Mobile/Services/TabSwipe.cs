namespace Kawwer.Mobile.Services;

/// <summary>
/// Switches between the bottom navigation tabs in response to horizontal swipes,
/// mirroring a tap on the adjacent tab in the tab bar.
/// </summary>
public static class TabSwipe
{
    public static void Handle(SwipeDirection direction)
    {
        var delta = direction switch
        {
            SwipeDirection.Left => 1,   // Swipe left moves to the tab on the right.
            SwipeDirection.Right => -1, // Swipe right moves to the tab on the left.
            _ => 0
        };

        if (delta == 0)
        {
            return;
        }

        // Never swipe-navigate while a subpage (settings, match details, ...) is pushed.
        if (Shell.Current?.Navigation?.NavigationStack is { Count: > 1 })
        {
            return;
        }

        // Only applies when the main tab bar is the current shell item.
        var tabBar = Shell.Current?.CurrentItem;
        if (tabBar?.Items is not { Count: > 1 } tabs)
        {
            return;
        }

        var index = tabs.IndexOf(tabBar.CurrentItem);
        var target = index + delta;
        if (index < 0 || target < 0 || target >= tabs.Count)
        {
            return;
        }

        tabBar.CurrentItem = tabs[target];
    }
}
