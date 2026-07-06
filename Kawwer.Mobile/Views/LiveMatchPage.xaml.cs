using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class LiveMatchPage : ContentPage
{
    private readonly LiveMatchViewModel _viewModel;

    public LiveMatchPage(LiveMatchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.SubscribeRealtime();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.UnsubscribeRealtime();
    }
}
