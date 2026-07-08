using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.SubscribeRealtime();

        // Show cached content instantly; only re-fetch the dashboard when the data is stale.
        // Pull-to-refresh always reloads.
        if (_viewModel.IsStale(TimeSpan.FromSeconds(30)))
        {
            _viewModel.LoadCommand.Execute(null);
        }
        else
        {
            // Always reconcile the unread badge on return — a notification may have arrived while
            // another tab was on screen, when Home's live signal isn't reloading the dashboard.
            _viewModel.RefreshUnreadCommand.Execute(null);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.UnsubscribeRealtime();
    }
}
