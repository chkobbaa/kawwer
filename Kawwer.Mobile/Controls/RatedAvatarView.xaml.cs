namespace Kawwer.Mobile.Controls;

/// <summary>
/// A circular <see cref="AvatarView"/> with the player's star rating curved across the top,
/// used on the profile heroes. Combines the avatar and a <see cref="CurvedStarsDrawable"/> so the
/// two stay geometrically aligned regardless of size.
/// </summary>
public partial class RatedAvatarView : ContentView
{
    private readonly CurvedStarsDrawable _drawable = new();

    public RatedAvatarView()
    {
        InitializeComponent();
        StarsCanvas.Drawable = _drawable;
        ApplyLayout();
        ApplyContent();
        ApplyRating();
    }

    public static readonly BindableProperty ImageUrlProperty = BindableProperty.Create(
        nameof(ImageUrl), typeof(string), typeof(RatedAvatarView), propertyChanged: (b, _, _) => ((RatedAvatarView)b).ApplyContent());

    public static readonly BindableProperty InitialsProperty = BindableProperty.Create(
        nameof(Initials), typeof(string), typeof(RatedAvatarView), propertyChanged: (b, _, _) => ((RatedAvatarView)b).ApplyContent());

    public static readonly BindableProperty RatingProperty = BindableProperty.Create(
        nameof(Rating), typeof(double), typeof(RatedAvatarView), 0d, propertyChanged: (b, _, _) => ((RatedAvatarView)b).ApplyRating());

    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(double), typeof(RatedAvatarView), 96d, propertyChanged: (b, _, _) => ((RatedAvatarView)b).ApplyLayout());

    public static readonly BindableProperty MaxStarsProperty = BindableProperty.Create(
        nameof(MaxStars), typeof(int), typeof(RatedAvatarView), 5, propertyChanged: (b, _, _) => ((RatedAvatarView)b).ApplyLayout());

    public string? ImageUrl
    {
        get => (string?)GetValue(ImageUrlProperty);
        set => SetValue(ImageUrlProperty, value);
    }

    public string? Initials
    {
        get => (string?)GetValue(InitialsProperty);
        set => SetValue(InitialsProperty, value);
    }

    /// <summary>Rating in the range [0, <see cref="MaxStars"/>]. 0 hides the stars entirely.</summary>
    public double Rating
    {
        get => (double)GetValue(RatingProperty);
        set => SetValue(RatingProperty, value);
    }

    /// <summary>Diameter of the avatar circle. The control reserves extra height for the star band.</summary>
    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public int MaxStars
    {
        get => (int)GetValue(MaxStarsProperty);
        set => SetValue(MaxStarsProperty, value);
    }

    private void ApplyLayout()
    {
        var a = Size;
        var starSize = a * 0.24;
        var gap = a * 0.05;

        // Reserve enough height above the avatar for the tallest (centre) star, and enough width
        // for the outermost stars to sit inside the canvas.
        Root.WidthRequest = a * 1.85;
        Root.HeightRequest = a + gap + starSize + 6;

        Avatar.Size = a;
        _drawable.AvatarDiameter = a;
        _drawable.MaxStars = MaxStars;
        StarsCanvas.Invalidate();
    }

    private void ApplyContent()
    {
        Avatar.ImageUrl = ImageUrl;
        Avatar.Initials = Initials;
    }

    private void ApplyRating()
    {
        _drawable.Rating = Rating;
        StarsCanvas.Invalidate();
    }
}
