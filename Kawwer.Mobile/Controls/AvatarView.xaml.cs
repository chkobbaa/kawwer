using Microsoft.Maui.Controls.Shapes;

namespace Kawwer.Mobile.Controls;

/// <summary>
/// A circular profile avatar. Renders <see cref="ImageUrl"/> when set (masked to a circle),
/// falling back to the person's <see cref="Initials"/>. Used everywhere a user is shown so the
/// uploaded photo appears consistently across the app.
/// </summary>
public partial class AvatarView : ContentView
{
    public AvatarView()
    {
        InitializeComponent();
        ApplySize();
        ApplyInitials();
        ApplyImage();
    }

    public static readonly BindableProperty ImageUrlProperty = BindableProperty.Create(
        nameof(ImageUrl), typeof(string), typeof(AvatarView), propertyChanged: (b, _, _) => ((AvatarView)b).ApplyImage());

    public static readonly BindableProperty InitialsProperty = BindableProperty.Create(
        nameof(Initials), typeof(string), typeof(AvatarView), propertyChanged: (b, _, _) => ((AvatarView)b).ApplyInitials());

    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(double), typeof(AvatarView), 44d, propertyChanged: (b, _, _) => ((AvatarView)b).ApplySize());

    /// <summary>Remote (or local) URL of the profile photo. Null/empty shows the initials fallback.</summary>
    public string? ImageUrl
    {
        get => (string?)GetValue(ImageUrlProperty);
        set => SetValue(ImageUrlProperty, value);
    }

    /// <summary>1-2 letter fallback shown when no photo is available.</summary>
    public string? Initials
    {
        get => (string?)GetValue(InitialsProperty);
        set => SetValue(InitialsProperty, value);
    }

    /// <summary>Diameter of the avatar in device-independent units.</summary>
    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    private void ApplySize()
    {
        Frame.WidthRequest = Size;
        Frame.HeightRequest = Size;
        FrameShape.CornerRadius = new CornerRadius(Size / 2);
        // Keep the initials proportional to the circle, with a sensible floor for tiny avatars.
        InitialsLabel.FontSize = Math.Max(10, Size * 0.38);
    }

    private void ApplyInitials() => InitialsLabel.Text = Initials;

    private void ApplyImage()
    {
        var hasPhoto = !string.IsNullOrWhiteSpace(ImageUrl);
        PhotoImage.Source = hasPhoto ? ImageUrl : null;
        PhotoImage.IsVisible = hasPhoto;
    }
}
