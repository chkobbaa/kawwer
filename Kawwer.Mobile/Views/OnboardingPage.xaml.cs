using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class OnboardingPage : ContentPage
{
    private readonly OnboardingViewModel _viewModel;

    public OnboardingPage(OnboardingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        _viewModel.StepChanged += OnStepChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await AnimateStepInAsync();
        await _viewModel.AppearingCommand.ExecuteAsync(null);
    }

    private async void OnStepChanged(object? sender, int step) => await AnimateStepInAsync();

    /// <summary>
    /// Slides and fades the current step's inputs in, with the heading following a beat later, for a
    /// tasteful, premium transition between steps.
    /// </summary>
    private async Task AnimateStepInAsync()
    {
        HeadingStack.Opacity = 0;
        HeadingStack.TranslationY = 16;
        StepHost.Opacity = 0;
        StepHost.TranslationX = 36;

        await Task.WhenAll(
            HeadingStack.FadeToAsync(1, 240, Easing.CubicOut),
            HeadingStack.TranslateToAsync(0, 0, 240, Easing.CubicOut));

        await Task.WhenAll(
            StepHost.FadeToAsync(1, 280, Easing.CubicOut),
            StepHost.TranslateToAsync(0, 0, 280, Easing.CubicOut));
    }
}
