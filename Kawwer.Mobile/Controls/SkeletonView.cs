namespace Kawwer.Mobile.Controls;

/// <summary>
/// A container that gently pulses its opacity, giving its child "bones" a shimmering skeleton
/// look while first-load data is fetched. The animation runs only while the view is attached and
/// visible, so it costs nothing once real content replaces it.
/// </summary>
public sealed class SkeletonView : ContentView
{
    private const string AnimationName = "kawwerSkeletonShimmer";
    private bool _isAttached;

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        _isAttached = Handler is not null;
        if (_isAttached)
        {
            StartShimmer();
        }
        else
        {
            this.AbortAnimation(AnimationName);
        }
    }

    private void StartShimmer()
    {
        var shimmer = new Animation(v => Opacity = v, 0.4, 1.0, Easing.SinInOut);
        shimmer.Commit(
            this,
            AnimationName,
            length: 850,
            repeat: () => _isAttached && IsVisible);
    }
}
