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

        // Show cached content instantly; only re-fetch when the data is stale.
        // Pull-to-refresh always reloads.
        if (_viewModel.IsStale(TimeSpan.FromSeconds(30)))
        {
            _viewModel.LoadCommand.Execute(null);
        }
    }
}
