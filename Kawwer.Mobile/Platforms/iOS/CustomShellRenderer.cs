using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using UIKit;

namespace kawwer;

/// <summary>
/// iOS bottom tab bar to match Android: icons only (no titles), with the icons nudged so they sit
/// vertically centered once the label is gone.
/// </summary>
public class CustomShellRenderer : ShellRenderer
{
    protected override IShellTabBarAppearanceTracker CreateTabBarAppearanceTracker()
        => new IconOnlyTabBarAppearanceTracker(base.CreateTabBarAppearanceTracker());
}

internal sealed class IconOnlyTabBarAppearanceTracker : IShellTabBarAppearanceTracker
{
    // Push the icon down from the top and let it drop below the (removed) label baseline so it
    // ends up centered in the bar.
    private static readonly UIEdgeInsets IconInsets = new(6f, 0f, -6f, 0f);

    private readonly IShellTabBarAppearanceTracker _inner;

    public IconOnlyTabBarAppearanceTracker(IShellTabBarAppearanceTracker inner) => _inner = inner;

    public void ResetAppearance(UITabBarController controller) => _inner.ResetAppearance(controller);

    public void SetAppearance(UITabBarController controller, ShellAppearance appearance)
    {
        _inner.SetAppearance(controller, appearance);
        ApplyIconOnly(controller.TabBar);
    }

    public void UpdateLayout(UITabBarController controller)
    {
        _inner.UpdateLayout(controller);
        ApplyIconOnly(controller.TabBar);
    }

    public void Dispose() => _inner.Dispose();

    private static void ApplyIconOnly(UITabBar? tabBar)
    {
        if (tabBar?.Items is null)
        {
            return;
        }

        foreach (var item in tabBar.Items)
        {
            // Empty (not null) so the layout still reserves the item; the icon then centers.
            item.Title = string.Empty;
            item.ImageInsets = IconInsets;
        }
    }
}
