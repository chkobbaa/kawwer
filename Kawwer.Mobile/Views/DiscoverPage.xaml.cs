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
        _viewModel.LoadCommand.Execute(null);
    }
}
