namespace Kawwer.Mobile.Controls;

/// <summary>
/// A friendly, on-brand empty state: a custom illustration, a short title, and a supporting line.
/// Replaces the app's blank lists and bare "No X." labels with a consistent, polished treatment.
/// </summary>
public partial class EmptyStateView : ContentView
{
    public EmptyStateView()
    {
        InitializeComponent();
        ApplyIllustration();
        ApplyText();
        ApplyIllustrationSize();
    }

    public static readonly BindableProperty IllustrationProperty = BindableProperty.Create(
        nameof(Illustration), typeof(string), typeof(EmptyStateView), propertyChanged: (b, _, _) => ((EmptyStateView)b).ApplyIllustration());

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(EmptyStateView), propertyChanged: (b, _, _) => ((EmptyStateView)b).ApplyText());

    public static readonly BindableProperty MessageProperty = BindableProperty.Create(
        nameof(Message), typeof(string), typeof(EmptyStateView), propertyChanged: (b, _, _) => ((EmptyStateView)b).ApplyText());

    public static readonly BindableProperty IllustrationSizeProperty = BindableProperty.Create(
        nameof(IllustrationSize), typeof(double), typeof(EmptyStateView), 150d, propertyChanged: (b, _, _) => ((EmptyStateView)b).ApplyIllustrationSize());

    /// <summary>Image source name of the illustration (e.g. "empty_matches.png").</summary>
    public string? Illustration
    {
        get => (string?)GetValue(IllustrationProperty);
        set => SetValue(IllustrationProperty, value);
    }

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Message
    {
        get => (string?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public double IllustrationSize
    {
        get => (double)GetValue(IllustrationSizeProperty);
        set => SetValue(IllustrationSizeProperty, value);
    }

    private void ApplyIllustration()
        => IllustrationImage.Source = string.IsNullOrWhiteSpace(Illustration) ? null : Illustration;

    private void ApplyText()
    {
        TitleLabel.Text = Title;
        MessageLabel.Text = Message;
    }

    private void ApplyIllustrationSize()
    {
        IllustrationImage.HeightRequest = IllustrationSize;
        IllustrationImage.WidthRequest = IllustrationSize;
    }
}
