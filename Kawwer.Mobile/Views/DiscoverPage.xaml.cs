using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class DiscoverPage : ContentPage
{
    private readonly DiscoverViewModel _viewModel;

    public DiscoverPage(DiscoverViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Show cached content instantly; only re-fetch when the data is stale.
        if (_viewModel.IsStale(TimeSpan.FromSeconds(30)))
        {
            _viewModel.LoadCommand.Execute(null);
        }
    }
}
