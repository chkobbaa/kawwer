using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class FriendsPage : ContentPage
{
    private readonly FriendsViewModel _viewModel;

    public FriendsPage(FriendsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.SubscribeRealtime();

        // Show cached content instantly; only re-fetch when the data is stale.
        if (_viewModel.IsStale(TimeSpan.FromSeconds(30)))
        {
            _viewModel.LoadCommand.Execute(null);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.UnsubscribeRealtime();
    }
}
