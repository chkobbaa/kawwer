using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class CreateMatchPage : ContentPage
{
    private readonly CreateMatchViewModel _viewModel;

    public CreateMatchPage(CreateMatchViewModel viewModel)
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
